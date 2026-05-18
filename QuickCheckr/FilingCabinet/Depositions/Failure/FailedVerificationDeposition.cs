namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public record FailedVerificationDeposition(string FailedExpectation) : FailureDeposition
{
    public override string GetFailureDescription() => FailedExpectation;
};