using QuickCheckr.UnderTheHood;

namespace QuickCheckr.FilingCabinet.Depositions;

public record InputDeposition(bool Labeled, string Label, object Value)
{
    public object? InputValue { get; init; } = default!;
    public Type Type { get; init; } = default!;
    public Maybe Original { get; init; } = Maybe.Nothing();
    public Maybe Redux { get; init; } = Maybe.Nothing();
    public Maybe<object> ReduxValue { get; init; } = Maybe<object>.Nothing();
    public int MaxLabelLength()
        => Math.Max(Label.Length, Redux.HasValue ? 5 : 0);
    public string GetReduxOrValue()
        => (Redux.HasValue ? Redux.Value : (string)Value)!;
    public object GetReduxOrValueAsObject()
        => (ReduxValue.HasValue ? ReduxValue.Value : InputValue)!;
}
