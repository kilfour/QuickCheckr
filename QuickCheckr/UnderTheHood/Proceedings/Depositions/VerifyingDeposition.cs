namespace QuickCheckr.UnderTheHood.Proceedings.Depositions;

public record VerifyingDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};