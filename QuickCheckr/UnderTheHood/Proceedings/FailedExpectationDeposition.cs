namespace QuickCheckr.UnderTheHood.Proceedings;

public record FailedExpectationDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};