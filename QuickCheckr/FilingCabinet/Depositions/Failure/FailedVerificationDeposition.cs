namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public sealed record FailedVerificationDeposition(string FailedExpectation, string[] Messages) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};
