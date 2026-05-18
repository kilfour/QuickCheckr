namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public record FailedExceptionDeposition(string Message, string FailureDescription) : FailureDeposition
{
    public override string GetFailureDescription() => FailureDescription;
};