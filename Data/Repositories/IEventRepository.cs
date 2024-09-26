using Domain;
using Domain.Interfaces;

namespace Data.Repositories;

public interface IEventRepository
{
    ValueTask<bool> CreateAsync<TEventData>(EventData<TEventData> @event, string partitionKey, CancellationToken cancellationToken)
        where TEventData : IDomainEvent;

    ValueTask<bool> UpsertAsync<TEventData>(EventData<TEventData> @event, string partitionKey, string etag, CancellationToken cancellationToken)
        where TEventData : IDomainEvent;
}