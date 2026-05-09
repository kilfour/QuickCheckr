using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public record NoWitnessForReducerWarning(string Key) : Warning(WarningLevel.Important)
{
    public override string Message => $"No witness for reducer at '{Key}'.";
}
