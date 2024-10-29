using Application.Builders;
using Data.Models;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Commands;

public abstract class BaseQuotaCommandHandler<TRequest, TResponse>(
    CosmosClient cosmosClient,
    IOptions<CosmosSettings> cosmosSettings,
    ILogger<BaseQuotaCommandHandler<TRequest, TResponse>> logger) : BasePartitionKeyBuilder, IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly Container Container = cosmosClient.GetContainer(
        cosmosSettings.Value.QuotaTableDatabase,
        cosmosSettings.Value.QuotaTableContainer
    );

    protected readonly ILogger<BaseQuotaCommandHandler<TRequest, TResponse>> Logger = logger;

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        => HandleCommandAsync(request, cancellationToken);

    protected abstract Task<TResponse> HandleCommandAsync(TRequest request, CancellationToken cancellationToken);
}