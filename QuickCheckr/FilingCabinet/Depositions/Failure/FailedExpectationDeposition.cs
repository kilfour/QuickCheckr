namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public sealed record FailedExpectationDeposition(string FailedExpectation, string[] Messages) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};
