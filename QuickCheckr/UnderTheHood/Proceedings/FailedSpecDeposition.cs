namespace QuickCheckr.UnderTheHood.Proceedings;

public record FailedSpecDeposition(string FailedSpec) : FailureDeposition
{
    public override string GetFailureDescription() => FailedSpec;
};