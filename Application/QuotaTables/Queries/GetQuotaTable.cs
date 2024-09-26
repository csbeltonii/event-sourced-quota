using Data.Models;
using Domain.Quota;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Queries;

public record GetQuotaTable(string Id, string ProjectId, string QuotaTableName) : IRequest<QuotaTable>;

public class GetQuotaTableHandler(
    CosmosClient cosmosClient, 
    IOptions<CosmosSettings> cosmosSettings,
    ILogger<BaseQuotaCollectionQuery<GetQuotaTable, QuotaTable>> logger) 
    : BaseQuotaCollectionQuery<GetQuotaTable, QuotaTable>(cosmosClient, cosmosSettings, logger) 
{
    protected override async Task<QuotaTable> HandleQueryAsync(GetQuotaTable request, CancellationToken cancellationToken)
    {
        var (id, projectId, quotaTableName) = request;

        var response = await Container.ReadItemAsync<QuotaTable>(
            id,
            GeneratePartitionKey(projectId, quotaTableName),
            cancellationToken: cancellationToken
        );

        LogRequestCharge(nameof(GetQuotaTableHandler), response.RequestCharge);

        return response;
    }
}