namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public record ShrinkingContext(CheckrState State, CheckrOf<Case> Check, bool UseDefaultReducers)
{
    public Verifier Verifier => new(State, Check);
    public string PrintInput(object? input) => State.InputPrinter.IntroduceThis(input)!;
    public string PrintRedux(object? input) => State.ReductionPrinter.IntroduceThis(input)!;
}
