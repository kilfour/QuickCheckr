namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public sealed record FailedExceptionDeposition(string FailureDescription, string StackTrace) : FailureDeposition
{
    public override string GetFailureDescription() => FailureDescription;
};
