namespace QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;

public record FailedExpectationDeposition(string FailedExpectation, List<string> Messages) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};