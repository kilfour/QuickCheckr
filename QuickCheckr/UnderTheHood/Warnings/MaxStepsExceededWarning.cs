using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public record MaxStepsExceededWarning(string Key, int NumberOfMaxSteps) : Warning(WarningLevel.Important)
{
    public override string Message => $"Reducer stopped at {Key}. Max steps ({NumberOfMaxSteps}) exceeded.";
}