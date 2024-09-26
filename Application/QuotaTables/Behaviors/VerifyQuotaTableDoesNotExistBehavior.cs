using Application.QuotaTables.Commands;
using Application.QuotaTables.Exceptions;
using Data.Models;
using Domain;
using Domain.Quota;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Behaviors;

public class VerifyQuotaTableDoesNotExistBehavior(
    CosmosClient cosmosClient,
    IOptions<CosmosSettings> cosmosSettings) : IPipelineBehavior<CreateQuotaTable, QuotaTable>
{
    private readonly Container _container = cosmosClient.GetContainer(cosmosSettings.Value.QuotaTableDatabase, cosmosSettings.Value.QuotaTableContainer);

    public async Task<QuotaTable> Handle(CreateQuotaTable request, RequestHandlerDelegate<QuotaTable> next, CancellationToken cancellationToken)
    {
        var (projectId, quotaTableName, _, _) = request;

        var partitionKey = new PartitionKeyBuilder()
                           .Add(projectId)
                           .Add(quotaTableName)
                           .Build();

        var existingQuotaTable = _container
                                 .GetItemLinqQueryable<QuotaTable>(requestOptions: new QueryRequestOptions
                                                                   {
                                                                       PartitionKey = partitionKey
                                                                   },
                                                                   linqSerializerOptions: new CosmosLinqSerializerOptions
                                                                   {
                                                                       PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase

                                                                   })
                                 .Where(quotaTable => quotaTable.QuotaTableName == quotaTableName &&
                                                      quotaTable.ProjectId == projectId &&
                                                      quotaTable.DocumentType == DocumentTypes.QuotaTable)
                                 .ToFeedIterator();

        while (existingQuotaTable.HasMoreResults)
        {
            var result = await existingQuotaTable.ReadNextAsync(cancellationToken);

            if (!result.Any())
            {
                continue;
            }

            var existingTable = result.Resource.FirstOrDefault();

            if (existingTable is not null)
            {
                throw new QuotaTableExistsException("Quota table already exists.");

            }
        }

        return await next();
    }
}