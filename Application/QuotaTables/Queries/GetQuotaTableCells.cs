using Data.Models;
using Domain;
using Domain.Quota;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Queries;

public record GetQuotaTableCells(string ProjectId, string QuotaTableName, int PageNumber, int PageSize) : IRequest<IEnumerable<QuotaCell>>;

public class GetQuotaTableCellsHandler(
    CosmosClient cosmosClient,
    IOptions<CosmosSettings> cosmosSettings,
    ILogger<BaseQuotaCollectionQuery<GetQuotaTableCells, IEnumerable<QuotaCell>>> logger)
    : BaseQuotaCollectionQuery<GetQuotaTableCells, IEnumerable<QuotaCell>>(cosmosClient, cosmosSettings, logger) 
{
    protected override async Task<IEnumerable<QuotaCell>> HandleQueryAsync(GetQuotaTableCells request, CancellationToken cancellationToken)
    {
        var (projectId, quotaTableName, pageNumber, pageSize) = request;
        var quotaCells = new List<QuotaCell>(pageSize);

        var iterator = Container
                       .GetItemLinqQueryable<QuotaCell>(
                           requestOptions: new QueryRequestOptions
                           {
                               PartitionKey = GeneratePartitionKey(projectId, quotaTableName),
                               MaxItemCount = pageSize
                           },
                           linqSerializerOptions: new CosmosLinqSerializerOptions()
                           {
                               PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                           }
                       )
                       .Where(
                           quotaCell => quotaCell.DocumentType == DocumentTypes.QuotaCell &&
                                        quotaCell.QuotaTableName == quotaTableName &&
                                        quotaCell.ProjectId == projectId
                       )
                       .Skip(pageNumber * pageSize)
                       .ToFeedIterator();

        while (iterator.HasMoreResults && quotaCells.Count != pageSize)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            LogRequestCharge(nameof(GetQuotaTableCellsHandler), response.RequestCharge);

            quotaCells.AddRange(response);
        }

        return quotaCells;
    }
}