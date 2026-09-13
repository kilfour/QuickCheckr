namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public sealed record FailedVerificationDeposition(string FailedExpectation, List<string> Messages) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};
