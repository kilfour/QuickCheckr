using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;

namespace QuickCheckr.Invariants;

public static class Check
{
    public static CaseFile ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs(), ActivateReducers);

    public static CaseFile ForAll<T>(int seed, FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(seed, ActivateReducers);

    public static CaseFile ForAll<T>(CheckrOfTRun.RunCount runs, FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(runs, ActivateReducers);

    public static CaseFile ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate, params ClassificationOption<T>[] classificationOptions)
        => (
            from input in Checkr.Input("Input", fuzzr, Shrink.OnType(classificationOptions))
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs(), ActivateReducers);

    public static CaseFile ForAll<T>(int seed, FuzzrOf<T> fuzzr, Func<T, bool> predicate, params ClassificationOption<T>[] classificationOptions)
        => (
            from input in Checkr.Input("Input", fuzzr, Shrink.OnType(classificationOptions))
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(seed, ActivateReducers);

    public static CaseFile ForAll<T>(int seed, FuzzrOf<T> fuzzr, Func<T, bool> predicate, params Func<Reducer<T>>[] reducers)
        => (
            from input in Checkr.Input("Input", fuzzr, Shrink.OnType(reducers))
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(seed, ActivateReducers);

    public static CaseFile ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate, params Func<Reducer<T>>[] reducers)
        => (
            from input in Checkr.Input("Input", fuzzr, Shrink.OnType(reducers))
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs(), ActivateReducers);

    private static CheckrConfig ActivateReducers(CheckrConfig config) => config with { ShrinkMode = config.ShrinkMode | ShrinkMode.Reduction };

}