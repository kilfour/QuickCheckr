using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public sealed record AllInputsIrrelevantWarning() : Warning(WarningLevel.Verbose)
{
    public override string Message => "All inputs were considered irrelevant.";
}
