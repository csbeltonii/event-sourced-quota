using System.Text.Json.Serialization;

namespace Domain.Quota;

public class QuotaCell(string coordinate, string condition, string quotaTableName, string projectId, string userId) : Entity(userId)
{
    [JsonConstructor]
    private QuotaCell() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) { }
    public override string DocumentType => DocumentTypes.QuotaCell;

    public new string Id => $"{ProjectId}@{QuotaTableName}@{Coordinate}";
    public string QuotaTableName { get; set; } = quotaTableName;
    public string ProjectId { get; set; } = projectId;
    public string Coordinate { get; set; } = coordinate;
    public string Condition { get; set; } = condition;
    public Target Target { get; set; } = InfiniteTarget.Create();

    public long Active { get; set; }
    public long Need { get; set; }
    public long Complete { get; set; }
    public long Overage { get; set; }
}