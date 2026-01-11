namespace QuickCheckr.UnderTheHood.Warnings;

public record AllInputsIrrelevantWarning : Warning
{
    public override string Message => "All inputs were considered irrelevant.";
}
