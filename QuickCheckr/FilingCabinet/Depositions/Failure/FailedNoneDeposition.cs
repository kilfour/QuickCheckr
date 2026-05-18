namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public record FailedNoneDeposition : FailureDeposition
{
    public override string GetFailureDescription() => string.Empty;
};