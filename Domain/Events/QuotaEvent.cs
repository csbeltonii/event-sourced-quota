using Domain.Interfaces;

namespace Domain.Events;

public abstract record QuotaEvent(string QuotaName, string CellName, string RespondentId) : IDomainEvent
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string CreatedBy { get; } = RespondentId;
}