using QuickFuzzr;

namespace QuickCheckr.Invariants;

public static class Check
{
    public static void ForAll<T>(FuzzrOf<T> fuzzr, Func<T, bool> predicate)
        => (
            from input in Checkr.Input("Input", fuzzr)
            from expectation in Checkr.Expect("Property", () => predicate(input))
            select Case.Closed)
        .Run(100.ExecutionsPerRun());
}