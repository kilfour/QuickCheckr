using System.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickFuzzr;

namespace QuickCheckr.Invariants;

public interface ITestrRunner
{
    CaseFile Run();
    CaseFile Run(int seed);
    CaseFile Run(CheckrOfTRun.RunCount tries);
}

public class TestrRunner<T>(
    FuzzrOf<T> fuzzr,
    Shrinker[] shrinkers,
    CheckrOf<Case>[] formatters,
    Func<T, bool> Invariant,
    Func<T, int>? Deliberation,
    int? DeliberationTarget,
    string testName,
    bool UseBuiltInReducers) : ITestrRunner
{
    private CheckrOf<Case> GetCheckr() =>
        from format in Combine.Checkrs(formatters)
        from input in Checkr.Input("Input", fuzzr, shrinkers)
        from trace in Checkr.Trace("Original", () => input)
        from run in Checkr.ActCarefully("Run", () => Invariant(input))
        from rethrow in Checkr.When(() => run.Threw, Checkr.Act("Rethrow", () => Invariant(input)))
        from expectation in Checkr.ExpectWhen(testName, () => !run.Threw, () => run.Value)
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
            ReportMode = a.ReportMode & ~ReportMode.Warning & ~ReportMode.Labels & ~ReportMode.StackTrace
        };
    }
}
