namespace QuickCheckr.UnderTheHood.Proceedings;

public record InputDeposition(
    bool Labeled,
    string Label,
    object Value,
    Maybe Original,
    Maybe Redux)
{
    public int MaxLabelLength()
        => Math.Max(Label.Length, Redux.HasValue ? 5 : 0);

    // public Maybe Original { get; init; } = Maybe.Nothing();
    // public Maybe Redux { get; init; } = Maybe.Nothing();
}
