using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickFuzzr;

namespace QuickCheckr.Invariants;

public static class Check
{
    public static void ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate, params IClassificationOption[] classificationOptions)
        => (
            from input in Checkr.Input("Input", fuzzr, classificationOptions)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs());

    public static void ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.Runs());

    public static void ForAll<T>(int seed, FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(seed);

    public static void ForAll<T>(CheckrOfTRun.RunCount runs, FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(runs);
}