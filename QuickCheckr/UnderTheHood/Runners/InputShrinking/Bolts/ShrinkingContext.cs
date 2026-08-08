using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public interface IWhistleblower
{
    void Warn(string key, Func<Warning> factory);
}

public interface IReductionSettings
{
    int MaxStepsPerReducer { get; }
}

public interface IReductionWhistleblower : IWhistleblower, IReductionSettings;

public record ShrinkingContext(CheckrState State, CheckrOf<Case> Check, bool UseDefaultReducers)
    : IReductionWhistleblower, IWhistleblower, IReductionSettings
{
    public int CollapseCount { get; set; }

    public Verifier Verifier => new(State, Check);

    public int MaxStepsPerReducer => State.MaxStepsPerReducer;

    public string PrintInput(object? input)
        => State.ClassificationFormatter.InputPrinter.IntroduceThis(input)!;

    public string PrintRedux(object? input)
        => State.ReductionPrinter.IntroduceThis(input)!;

    public void Warn(string key, Func<Warning> factory)
        => State.Memory.StoreWarning(key, factory);
}
