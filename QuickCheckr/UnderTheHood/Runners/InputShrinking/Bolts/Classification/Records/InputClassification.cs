namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public abstract record InputClassification
{
    public Maybe Redux { get; init; } = Maybe.Nothing();
    public Maybe<object> ReduxValue { get; init; } = Maybe<object>.Nothing();

    public virtual Maybe GetRedux() => Redux;
}
