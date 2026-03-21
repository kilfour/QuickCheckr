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

public record TestrBuilder0(string TestName)
{
    public TestrBuilder1<T> For<T>(FuzzrOf<T> fuzzr) => new(fuzzr, TestName);
}

public record TestrBuilder1<T>(FuzzrOf<T> Fuzzr, string TestName = "Invariant")
{
    private bool useBuiltInReducers = true;
    public TestrBuilder1<T> DisableValueReduction() { useBuiltInReducers = false; return this; }
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation)
        => new(Fuzzr, deliberation, null, TestName, useBuiltInReducers);
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation, int deliberationTarget)
        => new(Fuzzr, deliberation, deliberationTarget, TestName, useBuiltInReducers);
    public TestrBuilder3<T> Assert(Func<T, bool> invariant) =>
        new(Fuzzr, invariant, null, null, TestName, useBuiltInReducers);
}

public record TestrBuilder2<T>(
    FuzzrOf<T> Fuzzr,
    Func<T, int>? Deliberation,
    int? DeliberationTarget,
    string TestName,
    bool UseBuiltInReducers)
{
    public TestrBuilder3<T> Assert(Func<T, bool> invariant) =>
        new(Fuzzr, invariant, Deliberation, DeliberationTarget, TestName, UseBuiltInReducers);
}

public record TestrBuilder3<T>(
    FuzzrOf<T> Fuzzr,
    Func<T, bool> Invariant,
    Func<T, int>? Deliberation,
    int? DeliberationTarget,
    string TestName,
    bool UseBuiltInReducers)
{
    private CheckrOf<Case> GetCheckr() =>
        from input in Checkr.Input("Input", Fuzzr)
        from trace in Checkr.Trace("Original", () => input)
        from expectation in Checkr.Expect(TestName, () => Invariant(input))
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
