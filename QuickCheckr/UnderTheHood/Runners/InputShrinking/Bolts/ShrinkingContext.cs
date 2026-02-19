namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public record ShrinkingContext(CheckrState State, CheckrOf<Case> Check, bool UseDefaultReducers)
{
    public Verifier Verifier => new(State, Check);
}
