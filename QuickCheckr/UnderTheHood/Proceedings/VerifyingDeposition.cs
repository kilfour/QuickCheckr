namespace QuickCheckr.UnderTheHood.Proceedings;

public record VerifyingDeposition(string FailedSpec) : FailureDeposition
{
    public override string GetFailureDescription() => FailedSpec;
};