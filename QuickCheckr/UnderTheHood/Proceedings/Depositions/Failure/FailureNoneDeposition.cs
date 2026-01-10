namespace QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;

public record FailureNoneDeposition : FailureDeposition
{
    public override string GetFailureDescription() => string.Empty;
};