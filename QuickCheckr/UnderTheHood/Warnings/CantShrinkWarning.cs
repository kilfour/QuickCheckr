using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public sealed record CantShrinkWarning(Type Type, string Key) : Warning(WarningLevel.Verbose)
{
    public override string Message => $"Can't shrink type '{Type.Name}' at input '{Key}'.";
}
