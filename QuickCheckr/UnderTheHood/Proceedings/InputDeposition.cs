namespace QuickCheckr.UnderTheHood.Proceedings;

public record InputDeposition(bool Labeled, string Label, object Value)
{
    public int MaxLabelLength()
        => Math.Max(Label.Length, Redux.HasValue ? 5 : 0);

    public Maybe Original { get; init; } = Maybe.Nothing();
    public Maybe Redux { get; init; } = Maybe.Nothing();
    public string GetReduxOrValue()
        => (Redux.HasValue ? Redux.Value : (string)Value)!;
}
