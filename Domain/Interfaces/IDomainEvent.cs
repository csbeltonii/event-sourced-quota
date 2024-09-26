using MediatR;

namespace Domain.Interfaces;

public interface IDomainEvent : INotification
{
    public string Id { get; }
    public string CreatedBy { get; }
}