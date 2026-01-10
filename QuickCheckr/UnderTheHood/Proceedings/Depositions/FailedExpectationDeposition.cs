namespace QuickCheckr.UnderTheHood.Proceedings.Depositions;

public record FailedExpectationDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};