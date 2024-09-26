using System.Text.Json.Serialization;
using Domain.Events;
using Domain.Interfaces;

namespace Domain.Quota;

public class QuotaTable(string quotaTableName, string projectId, string userId) : DomainSnapshot(userId), IQuotaTable
{
    [JsonConstructor]
    private QuotaTable() : this(string.Empty, string.Empty, string.Empty) { }
    public override string DocumentType => DocumentTypes.QuotaTable;

    public string QuotaTableName { get; set; } = quotaTableName;
    public string ProjectId { get; set; } = projectId;

    public long Active { get; set; }
    public long Complete { get; set; }
    public long Overage { get; set; }

    public override void Apply(IDomainEvent @event)
    {
        if (!@event.GetType().IsAssignableFrom(typeof(QuotaEvent)))
        {
            throw new InvalidOperationException(
                $"Invalid domain event provided. Received {@event.GetType().BaseType}, Expected: {typeof(QuotaEvent)}"
            );
        }
    }
}