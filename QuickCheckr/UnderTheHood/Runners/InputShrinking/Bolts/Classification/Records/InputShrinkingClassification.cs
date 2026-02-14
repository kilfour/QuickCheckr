namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public abstract record InputShrinkingClassification
{
    public Maybe Redux { get; init; } = Maybe.Nothing();
    public Maybe<object> ReduxValue { get; init; } = Maybe<object>.Nothing();
}
