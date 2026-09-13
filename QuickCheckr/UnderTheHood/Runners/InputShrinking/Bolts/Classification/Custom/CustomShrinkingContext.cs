namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed record CustomShrinkingContext(ShrinkingInfo Info, ShrinkingContext Context)
{
    public string PrintInput(object? value)
        => Context.PrintInput(value);
}
