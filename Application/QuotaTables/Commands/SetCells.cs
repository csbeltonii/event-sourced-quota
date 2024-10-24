using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Application.Extensions;
using Application.Models;
using Application.QuotaTables.Exceptions;
using Azure;
using Data.Models;
using Domain;
using Domain.Quota;
using Domain.Utility;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Commands;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExecutionMode
{
    [EnumMember(Value = "point-read")]
    PointRead,

    [EnumMember(Value = "point-read-many")]
    PointReadMany,

    [EnumMember(Value = "iterator")]
    Iterator
}

public record SetCells(
    string ProjectId,
    string QuotaTableName, 
    IReadOnlyList<string> Coordinates, 
    ExecutionMode executionMode) : IRequest<Result>;

public class SetCellsHandler(
    CosmosClient cosmosClient,
    IOptions<CosmosSettings> cosmosSettings,
    IOptionsMonitor<BatchingSettings> batchSettings,
    ILogger<BaseQuotaCommandHandler<SetCells, Result>> logger) 
    : BaseQuotaCommandHandler<SetCells, Result>(cosmosClient, cosmosSettings, logger) 
{
    protected override async Task<Result> HandleCommandAsync(SetCells request, CancellationToken cancellationToken)
    {
        var (projectId, quotaTableName, coordinates, executionMode) = request;
        var partitionKey = GeneratePartitionKey(projectId, quotaTableName);
        var transaction = Container.CreateTransactionalBatch(partitionKey);

        var getQuotaTable = await Container.ReadItemAsync<QuotaTable>(
            id: $"{projectId}@{quotaTableName}",
            partitionKey,
            cancellationToken: cancellationToken
        );

        var quotaTable = getQuotaTable.Resource;

        if (coordinates.Count > quotaTable.CellCount)
        {
            return Result.Failure("More coordinates requested to be set than exist on the table.");
        }

        var quotaCells = executionMode switch
        {
            ExecutionMode.Iterator => await GetQuotaCellsByQueryAsync(
                projectId,
                coordinates,
                partitionKey,
                cancellationToken
            ),
            ExecutionMode.PointRead => await GetQuotaCellsByPointReadAsync(
                projectId, 
                quotaTableName,
                coordinates, 
                partitionKey,
                cancellationToken),
            ExecutionMode.PointReadMany => await GetQuotaCellsByPointReadManyAsync(
                projectId,
                quotaTableName,
                coordinates,
                partitionKey,
                cancellationToken),
            _                           => throw new ArgumentOutOfRangeException()
        };

        quotaTable.Active++;
        transaction.UpsertItem(quotaTable);
        var quotaTableUpdateResponse = await transaction.ExecuteAsync(cancellationToken);

        if (!quotaTableUpdateResponse.IsSuccessStatusCode)
        {
            throw new GenericQuotaException("Unable to save quota table.");
        }

        logger.LogInformation(
            "{ClassName}: Successfully updated quota table {QuotaTableName}",
            nameof(SetCells),
            quotaTableName
        );

        var batches = quotaCells.BatchCells(batchSettings.CurrentValue.BatchSize);

        await Parallel.ForEachAsync(
            batches,
            cancellationToken,
            async (batch, ctx) =>
            {
                var batchTransaction = Container.CreateTransactionalBatch(partitionKey);

                var transactionList = batch.Select(
                             quotaCell =>
                             {
                                 quotaCell.Active++;

                                 return batchTransaction.UpsertItem(quotaCell);
                             })
                         .ToList();

                logger.LogInformation(
                    "{ClassName}: Processing batch with {BatchSize} transactions.",
                    nameof(SetCells),
                    transactionList.Count
                );

                var response = await batchTransaction.ExecuteAsync(ctx);
                LogTotalRequestCharge(nameof(HandleCommandAsync), response.RequestCharge);
            }
        );


        return Result.Success();
    }

    private async Task<List<QuotaCell>> GetQuotaCellsByPointReadManyAsync(
        string projectId,
        string quotaTableName,
        IReadOnlyList<string> coordinates, 
        PartitionKey partitionKey,
        CancellationToken cancellationToken)
    {
        var coordinateIds = coordinates
                            .Select(coordinate => ($"{projectId}@{quotaTableName}@{coordinate}", partitionKey))
                            .ToList();

        var feedResponse = await Container.ReadManyItemsAsync<QuotaCell>(coordinateIds, cancellationToken: cancellationToken);

        LogTotalRequestCharge(nameof(GetQuotaCellsByPointReadAsync), feedResponse.RequestCharge);

        return feedResponse.ToList();
    }

    private async Task<List<QuotaCell>> GetQuotaCellsByQueryAsync(
        string projectId,
        IReadOnlyList<string> coordinates,
        PartitionKey partitionKey,
        CancellationToken cancellationToken)
    {
        var totalRuCost = 0.00;

        var cellQueryIterator = Container.GetItemLinqQueryable<QuotaCell>(
                                             requestOptions: new QueryRequestOptions
                                             {
                                                 PartitionKey = partitionKey
                                             },
                                             linqSerializerOptions: new CosmosLinqSerializerOptions
                                             {
                                                 PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                                             })
                                         .Where(
                                             quotaCell => quotaCell.DocumentType == DocumentTypes.QuotaCell &&
                                                          quotaCell.ProjectId == projectId &&
                                                          coordinates.Contains(quotaCell.Coordinate)
                                         )
                                         .ToFeedIterator();

        var quotaCells = new List<QuotaCell>(coordinates.Count);

        while (cellQueryIterator.HasMoreResults)
        {
            var batch = await cellQueryIterator.ReadNextAsync(cancellationToken);

            totalRuCost += batch.RequestCharge;

            quotaCells.AddRange(batch);
        }

        LogTotalRequestCharge(nameof(GetQuotaCellsByQueryAsync), totalRuCost);

        return quotaCells;
    }

    private async Task<List<QuotaCell>> GetQuotaCellsByPointReadAsync(
        string projectId,
        string quotaTableName,
        IReadOnlyList<string> coordinates,
        PartitionKey partitionKey,
        CancellationToken cancellationToken)
    {
        var totalRuCost = 0.00;
        var totalCalculationSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        var queries = coordinates
            .Select(
                async coordinate =>
                {
                    var id = $"{projectId}@{quotaTableName}@{coordinate}";
                    var response = await Container.ReadItemAsync<QuotaCell>(
                        id,
                        partitionKey,
                        cancellationToken: cancellationToken
                    );

                    try
                    {
                        await totalCalculationSemaphore.WaitAsync(cancellationToken);
                        totalRuCost += response.RequestCharge;
                    }
                    finally
                    {
                        totalCalculationSemaphore.Release();
                    }

                    return response;
                }
            );

        var result =  await Task.WhenAll(queries);

        LogTotalRequestCharge(nameof(GetQuotaCellsByPointReadAsync), totalRuCost);

        return result.Select(item => item.Resource).ToList();
    }

    private void LogTotalRequestCharge(string methodName, double requestCharge) =>
        Logger.LogInformation(
            "{ClassName}: {MethodName} completed. Total RU charge: {RequestCharge}",
            nameof(SetCells),
            methodName,
            requestCharge
        );
}