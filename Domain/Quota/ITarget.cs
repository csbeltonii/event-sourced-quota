namespace Domain.Quota;

public interface ITarget
{
    public long? Minimum { get; }
    public long? Maximum { get; }
    public TargetType Type { get; }
}