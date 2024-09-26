using Domain.Interfaces;

namespace Domain;

public abstract class DomainSnapshot(string userId) : Entity(userId)
{
    public abstract void Apply(IDomainEvent @event);
}