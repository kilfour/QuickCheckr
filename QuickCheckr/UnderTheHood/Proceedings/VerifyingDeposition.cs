namespace QuickCheckr.UnderTheHood.Proceedings;

public record VerifyingDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};