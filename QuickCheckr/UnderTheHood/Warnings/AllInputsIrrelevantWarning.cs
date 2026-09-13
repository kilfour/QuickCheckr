using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public sealed record AllInputsIrrelevantWarning() : Warning(WarningLevel.Important)
{
    public override string Message => "All inputs were considered irrelevant.";
}
