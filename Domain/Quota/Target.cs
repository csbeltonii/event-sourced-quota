namespace Domain.Quota;

public abstract class Target : ITarget
{
    public long? Minimum { get; set; }
    public long? Maximum { get; set; }
    public abstract TargetType Type { get; }
}