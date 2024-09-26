using Domain;
using System.Diagnostics;
using System.Net;
using Data.Logging;
using Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos;

namespace Data.Repositories;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : Entity
{
    private readonly Container Container;
    private readonly ILogger Logger;

    public Repository(
        CosmosClient cosmosClient,
        IOptions<CosmosSettings> cosmosSettings,
        ILogger<Repository<TEntity>> logger)
    {
        Logger = logger;
        Container = cosmosClient.GetContainer(cosmosSettings.Value.QuotaTableDatabase, cosmosSettings.Value.QuotaTableContainer);
    }

    public async Task<TEntity> CreateAsync(TEntity entity, string partitionKey, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await Container
            .CreateItemAsync(entity, new PartitionKey(partitionKey), cancellationToken: cancellationToken);

        Logger.LogStatistics(
            nameof(CreateAsync),
            typeof(TEntity).Name,
            partitionKey,
            response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            response.RequestCharge
        );

        return response.Resource;
    }

    public async Task<TEntity> Get(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await Container.ReadItemAsync<TEntity>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            Logger.LogStatistics(
                nameof(Get),
                typeof(TEntity).Name,
                partitionKey,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException cex) when (cex.StatusCode == HttpStatusCode.NotFound)
        {
            Logger.LogError(
                cex,
                "Unable to find resource with ID {EntityId}",
                id
            );

            return null;
        }
    }

    public async Task<TEntity> UpsertAsync(TEntity entity, string partitionKey, string etag, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var options = new ItemRequestOptions
            {
                IfMatchEtag = etag
            };

            var response = await Container.UpsertItemAsync(
                entity,
                new PartitionKey(partitionKey),
                options,
                cancellationToken
            );

            Logger.LogStatistics(
                nameof(UpsertAsync),
                typeof(TEntity).Name,
                partitionKey,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                response.RequestCharge
            );

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            Logger.LogError(
                ex,
                "Entity {EntityId} not found.",
                entity.Id
            );

            return default;
        }
        catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.PreconditionFailed)
        {
            Logger.LogError(
                ex,
                "Entity {EntityId} has been updated. Please retry update.",
                entity.Id
            );

            return default;
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "An error occurred while updating entity {EntityId}",
                entity.Id
            );

            throw;
        }

    }

    public async Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await Container.DeleteItemAsync<TEntity>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken)
                                          .ConfigureAwait(false);

            Logger.LogStatistics(
                nameof(DeleteAsync),
                typeof(TEntity).Name,
                partitionKey,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                response.RequestCharge
            );

            return true;
        }
        catch (CosmosException ex)
            when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            Logger.LogError(
                ex,
                "Entity {Entity} was not found.",
                partitionKey
            );

            return false;
        }
    }
}