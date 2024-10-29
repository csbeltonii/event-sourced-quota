using Application.Builders;
using Data.Models;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Commands;

public abstract class BaseQuotaEventHandler<TNotification>(
    CosmosClient cosmosClient,
    IOptions<CosmosSettings> cosmosSettings,
    ILogger<BaseQuotaEventHandler<TNotification>> logger)
    : BasePartitionKeyBuilder, INotificationHandler<TNotification>
    where TNotification : INotification
{
    protected readonly Container Container = cosmosClient.GetContainer(
        cosmosSettings.Value.QuotaTableDatabase,
        cosmosSettings.Value.QuotaTableContainer
    );

    protected readonly ILogger<BaseQuotaEventHandler<TNotification>> Logger = logger;

    public Task Handle(TNotification notification, CancellationToken cancellationToken)
        => HandleEventAsync(notification, cancellationToken);

    protected abstract Task HandleEventAsync(TNotification notification, CancellationToken cancellationToken);
}