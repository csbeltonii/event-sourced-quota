namespace Domain.Quota;

public class InfiniteTarget : Target
{
    public override TargetType Type => TargetType.Infinite;
    public static InfiniteTarget Create() => new();
}