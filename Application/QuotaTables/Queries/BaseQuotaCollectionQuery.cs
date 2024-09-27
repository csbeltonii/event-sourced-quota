using Data.Models;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Queries;

public abstract class BaseQuotaCollectionQuery<TRequest, TResponse>(
    CosmosClient cosmosClient,
    IOptions<CosmosSettings> cosmosSettings,
    ILogger<BaseQuotaCollectionQuery<TRequest, TResponse>> logger)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly Container Container = cosmosClient.GetContainer(
        cosmosSettings.Value.QuotaTableDatabase,
        cosmosSettings.Value.QuotaTableContainer);

    protected readonly ILogger Logger = logger;

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        => HandleQueryAsync(request, cancellationToken);

    protected abstract Task<TResponse> HandleQueryAsync(TRequest request, CancellationToken cancellationToken);

    protected PartitionKey GeneratePartitionKey(string projectId, string quotaTableName)
        => new PartitionKeyBuilder()
           .Add(projectId)
           .Add(quotaTableName)
           .Build();

    protected void LogRequestCharge(string className, double requestCharge)
        => Logger.LogInformation("{ClassName}: Retrieved item. Request charge: {RequestCharge}",
                                 className,
                                 requestCharge);
}