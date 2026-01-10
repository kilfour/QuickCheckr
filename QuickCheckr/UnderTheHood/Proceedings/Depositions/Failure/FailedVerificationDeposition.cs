namespace QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;

public record FailedVerificationDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};