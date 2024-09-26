using Microsoft.Azure.Cosmos;
using System.Diagnostics;
using System.Net;
using Domain;
using Microsoft.Extensions.Options;
using Data.Models;
using Data.Logging;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

public class EventRepository(
    CosmosClient cosmosClient,
    IOptions<CosmosSettings> cosmosSettings,
    ILogger<EventRepository> logger)
    : IEventRepository
{
    protected readonly Container Container = cosmosClient.GetContainer(
        cosmosSettings.Value.QuotaEventDatabase,
        cosmosSettings.Value.QuotaEventContainer);
    protected readonly ILogger Logger = logger;

    public async ValueTask<bool> CreateAsync<TEventData>(EventData<TEventData> @event, string partitionKey, CancellationToken cancellationToken)
        where TEventData : IDomainEvent
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await Container
            .CreateItemAsync(@event, new PartitionKey(partitionKey), cancellationToken: cancellationToken);

        Logger.LogStatistics(
            nameof(CreateAsync),
            typeof(TEventData).Name,
            @event.Id,
            response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            response.RequestCharge
        );

        return true;
    }

    public async ValueTask<bool> UpsertAsync<TEventData>(EventData<TEventData> @event, string partitionKey, string etag, CancellationToken cancellationToken)
        where TEventData : IDomainEvent
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var options = new ItemRequestOptions
            {
                IfMatchEtag = etag
            };

            var response = await Container.UpsertItemAsync(
                @event,
                new PartitionKey(partitionKey),
                options,
                cancellationToken
            );

            Logger.LogStatistics(
                nameof(UpsertAsync),
                typeof(TEventData).Name,
                @event.Id,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                response.RequestCharge
            );

            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            Logger.LogError(
                ex,
                "Entity {EntityId} not found.",
                @event.Id
            );

            return default;
        }
        catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.PreconditionFailed)
        {
            Logger.LogError(
                ex,
                "Entity {Entity} has been updated. Please retry update.",
                @event.Id
            );

            return default;
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "An error occurred while updating entity {Entity}",
                @event.Id
            );

            throw;
        }
    }
}