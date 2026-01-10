namespace QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;

public record FailedVerifierDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};