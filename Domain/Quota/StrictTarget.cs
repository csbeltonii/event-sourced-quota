namespace Domain.Quota;

public class StrictTarget : Target
{
    public override TargetType Type => TargetType.Strict;

    public StrictTarget(long minimum, long maximum) => (Minimum, Maximum) = (minimum, maximum);

    public StrictTarget Create(long minimum, long maximum) => new(minimum, maximum);
}