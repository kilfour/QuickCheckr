using System.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;

namespace QuickCheckr.Invariants;


public static class Testr
{
    public static TestrBuilder0 Named(string name) => new(name);
    public static TestrBuilder1<T> For<T>(FuzzrOf<T> fuzzr) => new(fuzzr);
}

public class TestrBuilder0(string testName)
{
    public TestrBuilder1<T> For<T>(FuzzrOf<T> fuzzr) => new(fuzzr, testName);
}

public class TestrBuilder1<T>(FuzzrOf<T> fuzzr, string testName = "Invariant")
{
    private bool useBuiltInReducers = true;
    public TestrBuilder1<T> DisableValueReduction() { useBuiltInReducers = false; return this; }
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation)
        => new(fuzzr, deliberation, null, testName, useBuiltInReducers);
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation, int deliberationTarget)
        => new(fuzzr, deliberation, deliberationTarget, testName, useBuiltInReducers);
    public TestrRunner<T> Assert(Func<T, bool> invariant) =>
        new(fuzzr, invariant, null, null, testName, useBuiltInReducers);
}

public class TestrBuilder2<T>(
    FuzzrOf<T> fuzzr,
    Func<T, int>? deliberation,
    int? deliberationTarget,
    string testName,
    bool useBuiltInReducers)
{
    public TestrRunner<T> Assert(Func<T, bool> invariant) =>
        new(fuzzr, invariant, deliberation, deliberationTarget, testName, useBuiltInReducers);
}

public class TestrRunner<T>(
    FuzzrOf<T> fuzzr,
    Func<T, bool> Invariant,
    Func<T, int>? Deliberation,
    int? DeliberationTarget,
    string testName,
    bool UseBuiltInReducers)
{
    private CheckrOf<Case> GetCheckr() =>
        from input in Checkr.Input("Input", fuzzr)
        from trace in Checkr.Trace("Original", () => input)
        from expectation in Checkr.Expect(testName, () => Invariant(input))
        select Case.Closed;

    [StackTraceHidden]
    public CaseFile Run()
        => Run(100.Runs());

    [StackTraceHidden]
    public CaseFile Run(int seed)
        => GetCheckr().Run(seed, GetConfig());

    [StackTraceHidden]
    public CaseFile Run(CheckrOfTRun.RunCount tries)
        => GetCheckr().Run(tries, GetConfig());

    private Func<CheckrConfig, CheckrConfig> GetConfig()
    {
        return a => a with
        {
            DeliberationPolicy = Deliberation == null ? null :
                a => a.InputsNamed<T>("Input", a => Deliberation(a)),
            DeliberationTarget = DeliberationTarget == null ? null : DeliberationTarget,
            ShrinkMode = UseBuiltInReducers ? a.ShrinkMode | ShrinkMode.Reduction : a.ShrinkMode,
            ReportMode = a.ReportMode & ~ReportMode.Warning & ~ReportMode.Labels
        };
    }
}
