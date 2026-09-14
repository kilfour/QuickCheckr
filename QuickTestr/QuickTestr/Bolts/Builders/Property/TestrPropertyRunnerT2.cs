using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Bolts.Builders.Property;

/// <summary>
/// Runs a property-based Testr against generated inputs.
/// Use for Testrs defined with a boolean invariant through Assert.
/// </summary>
public class TestrPropertyRunnerT2<TInput1, TInput2>(
    FuzzrOf<TInput1> fuzzrOfT1,
    FuzzrOf<TInput2> fuzzrOfT2,
    Shrinker[] shrinkers,
    CheckrOf<Case>[] formatters,
    Func<TInput1, TInput2, bool> Invariant,
    Func<TInput1, TInput2, int>? Deliberation,
    int? DeliberationTarget,
    string testName,
    bool UseBuiltInReducers) : TestrRunner<(TInput1, TInput2)>
{
    protected override string TestName { get; } = testName;

    protected override CheckrOf<Case> GetCheckr(
        Func<(TInput1, TInput2), object>? distinguish = null,
        Func<(TInput1, TInput2), bool>? rejectCaseWhen = null) =>
        from showr in Showr.ForInput()
        from format in Combine.Checkrs(formatters)
        from input in Checkr.Input("Input", Fuzzr.Tuple(fuzzrOfT1, fuzzrOfT2), shrinkers)
        from distinguishCase in Checkr.When(
            () => distinguish is not null,
            Checkr.Distinguish(() => distinguish!(input)))
        from rejectCase in Checkr.When(
            () => rejectCaseWhen is not null,
            Checkr.RejectCaseWhen(() => rejectCaseWhen!(input)))
        from deliberate in Checkr.When(
            () => Deliberation is not null,
            Checkr.Deliberate(() => Deliberation!(input.Item1, input.Item2)))
        from deliberateUntil in Checkr.When(
            () => DeliberationTarget.HasValue,
            Trackr.DeliberateUntil(DeliberationTarget.GetValueOrDefault()))
        from run in Checkr.ActCarefully("Run", () => Invariant(input.Item1, input.Item2))
        from rethrow in Checkr.When(() => run.Threw, Checkr.Act("Rethrow", () => Invariant(input.Item1, input.Item2)))
        from expectation in Checkr.ExpectWhen(TestName, () => !run.Threw, () => run.Value)
        select Case.Closed;

    protected override Func<CheckrConfig, CheckrConfig> GetConfig()
    {
        return a => a with
        {
            FileAs = fileName,
            Clerk = new PropertyClerk(),
            Custodian = custodian is null ? Custodian.Default : custodian,
            ShrinkMode = UseBuiltInReducers ? a.ShrinkMode | ShrinkMode.Reduction : a.ShrinkMode,
            ReportMode = a.ReportMode
        };
    }
}
