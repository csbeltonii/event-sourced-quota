using Domain;

namespace Data.Repositories;

public interface IRepository<TEntity>
    where TEntity : Entity
{
    Task<TEntity> CreateAsync(TEntity entity, string partitionKey, CancellationToken cancellationToken = default);
    Task<TEntity> Get(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task<TEntity> UpsertAsync(TEntity entity, string partitionKey, string etag, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
}