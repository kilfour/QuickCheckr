namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public record FailedExceptionDeposition(string FailureDescription, string StackTrace) : FailureDeposition
{
    public override string GetFailureDescription() => FailureDescription;
};