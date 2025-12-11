namespace QuickCheckr.UnderTheHood.Proceedings;

public record AssayerDeposition(string FailedSpec) : FailureDeposition
{
    public override string GetFailureDescription() => FailedSpec;
};