﻿using System.Net;
using Application.QuotaTables.Exceptions;
using Data.Models;
using Domain.Quota;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Commands;

public record CreateQuotaTable(
    string ProjectId,
    string QuotaTableName,
    int CellCount,
    string RequestedBy) : IRequest<QuotaTable>;

public class CreateQuotaTableHandler(
    CosmosClient cosmosClient,
    ILogger<CreateQuotaTableHandler> logger,
    IOptions<CosmosSettings> cosmosSettings)
    : IRequestHandler<CreateQuotaTable, QuotaTable>
{
    private readonly Container _container = cosmosClient.GetContainer(cosmosSettings.Value.QuotaTableDatabase, cosmosSettings.Value.QuotaTableContainer);

    public async Task<QuotaTable> Handle(CreateQuotaTable request, CancellationToken cancellationToken)
    {
        var (projectId, quotaTableName, cellCount, requestedBy) = request;
        var partitionKey = new PartitionKeyBuilder()
                           .Add(projectId)
                           .Add(quotaTableName)
                           .Build();

        var quotaTable = new QuotaTable(quotaTableName, projectId, requestedBy);
        var quotaTableCellBatches = BatchCells(GenerateQuotaCells(cellCount, projectId, quotaTableName, requestedBy), 50);
        var createTableResponse = await _container.CreateItemAsync(quotaTable, partitionKey, cancellationToken: cancellationToken);

        if (createTableResponse.StatusCode is not HttpStatusCode.Created)
        {
            throw new GenericQuotaException("Unable to create quota table.");
        }

        logger.LogInformation(
            "{ClassName}: Created quota table {QuotaTableName} in {Time}ms. Request Charge: {RequestCharge}",
            nameof(CreateQuotaTableHandler),
            quotaTableName,
            createTableResponse.Diagnostics.GetClientElapsedTime().TotalMilliseconds.ToString("F2"),
            createTableResponse.RequestCharge
        );

        await Parallel.ForEachAsync(
            quotaTableCellBatches,
            cancellationToken,
            async (batch, ctx) =>
            {
                var transaction = _container.CreateTransactionalBatch(partitionKey);
                _ = batch.Select(quotaCell => transaction.CreateItem(quotaCell)).ToList();
                var batchTransactionResponse = await transaction.ExecuteAsync(cancellationToken);

                logger.LogInformation(
                    "{ClassName}: Committed cell batch for {QuotaTableName} in {Time}ms. Request Charge: {RequestCharge}",
                    nameof(CreateQuotaTableHandler),
                    quotaTableName,
                    batchTransactionResponse.Diagnostics.GetClientElapsedTime().TotalMilliseconds.ToString("F2"),
                    batchTransactionResponse.RequestCharge
                );
            }
        );

        return quotaTable;
    }

    private static IEnumerable<QuotaCell> GenerateQuotaCells(
        int count,
        string projectId,
        string quotaTableName,
        string requestedBy)
    {
        while (count > 0)
        {
            yield return new QuotaCell(quotaTableName, projectId, requestedBy);
            count--;
        }
    }

    private static IEnumerable<IEnumerable<QuotaCell>> BatchCells(IEnumerable<QuotaCell> quotaCells, int batchSize)
    {
        var batch = new List<QuotaCell>(batchSize);

        foreach (var cell in quotaCells)
        {
            batch.Add(cell);

            if (batch.Count != batchSize)
            {
                continue;
            }

            yield return batch;
            batch = [];
        }
    }
}