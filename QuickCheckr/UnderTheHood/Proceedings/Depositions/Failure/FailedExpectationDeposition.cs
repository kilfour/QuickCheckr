namespace QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;

public record FailedExpectationDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};