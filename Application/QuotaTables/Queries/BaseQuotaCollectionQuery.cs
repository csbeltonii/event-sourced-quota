using Application.Builders;
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
    : BasePartitionKeyBuilder, IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly Container Container = cosmosClient.GetContainer(
        cosmosSettings.Value.QuotaTableDatabase,
        cosmosSettings.Value.QuotaTableContainer);

    protected readonly ILogger<BaseQuotaCollectionQuery<TRequest, TResponse>> Logger = logger;

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        => HandleQueryAsync(request, cancellationToken);

    protected abstract Task<TResponse> HandleQueryAsync(TRequest request, CancellationToken cancellationToken);

    protected void LogRequestCharge(string className, double requestCharge)
        => Logger.LogInformation("{ClassName}: Retrieved item. Request charge: {RequestCharge}",
                                 className,
                                 requestCharge);
}