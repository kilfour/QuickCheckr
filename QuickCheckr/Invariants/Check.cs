using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;

namespace QuickCheckr.Invariants;

public static class Check
{
    public static void ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs(), ActivateReducers);

    public static void ForAll<T>(int seed, FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(seed, ActivateReducers);

    public static void ForAll<T>(CheckrOfTRun.RunCount runs, FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(runs, ActivateReducers);

    public static void ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate, params IClassificationOption[] classificationOptions)
        => (
            from input in Checkr.Input("Input", fuzzr, classificationOptions)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs(), ActivateReducers);

    public static void ForAll<T>(int seed, FuzzrOf<T> fuzzr, Func<T, bool> predicate, params IClassificationOption[] classificationOptions)
        => (
            from input in Checkr.Input("Input", fuzzr, classificationOptions)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(seed, ActivateReducers);

    public static void ForAll<T>(int seed, FuzzrOf<T> fuzzr, Func<T, bool> predicate, params Func<Reducer<T>>[] reducers)
        => (
            from input in Checkr.Input("Input", fuzzr, reducers)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(seed, ActivateReducers);

    public static void ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate, params Func<Reducer<T>>[] reducers)
        => (
            from input in Checkr.Input("Input", fuzzr, reducers)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs(), ActivateReducers);

    private static CheckrConfig ActivateReducers(CheckrConfig config) => config with { ShrinkMode = config.ShrinkMode | ShrinkMode.Reduction };

}