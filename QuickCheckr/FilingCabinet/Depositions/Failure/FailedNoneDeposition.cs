namespace QuickCheckr.FilingCabinet.Depositions.Failure;

public sealed record FailedNoneDeposition : FailureDeposition
{
    public override string GetFailureDescription() => string.Empty;
};
