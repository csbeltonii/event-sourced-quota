using System.Net;
using Application.QuotaTables.Exceptions;
using Data.Models;
using Domain.Quota;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Commands;

public class IncrementQuotaTable(
    CosmosClient cosmosClient, 
    IOptions<CosmosSettings> cosmosSettings,
    ILogger<BaseQuotaEventHandler<RespondentQualified>> logger)
    : BaseQuotaEventHandler<RespondentQualified>(cosmosClient, cosmosSettings, logger)
{
    protected override async Task HandleEventAsync(RespondentQualified notification, CancellationToken cancellationToken)
    {
        var (projectId, quotaTableName) = notification;
        var partitionKey = GeneratePartitionKey(projectId, quotaTableName);

        var getQuotaTable = await Container.ReadItemAsync<QuotaTable>(
            $"{projectId}@{quotaTableName}",
            partitionKey,
            cancellationToken: cancellationToken
        );

        if (getQuotaTable is not { StatusCode: HttpStatusCode.OK })
        {
            throw new GenericQuotaException($"Unable to retrieve quota table {quotaTableName}");
        }

        var quotaTable = getQuotaTable.Resource;
        quotaTable.Active++;

        var response = await Container.UpsertItemAsync(
            quotaTable,
            partitionKey,
            new ItemRequestOptions
            {
                IfMatchEtag = quotaTable.Etag
            },
            cancellationToken: cancellationToken
        );

        if (response is not { StatusCode: HttpStatusCode.OK })
        {
            throw new GenericQuotaException($"Unable to save quota table {quotaTableName}");
        }
    }
}