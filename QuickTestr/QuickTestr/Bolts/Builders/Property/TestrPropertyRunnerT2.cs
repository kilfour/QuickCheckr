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
public sealed class TestrPropertyRunnerT2<TInput1, TInput2> : TestrRunner<(TInput1, TInput2)>
{
    private readonly FuzzrOf<TInput1> fuzzrOfT1;
    private readonly FuzzrOf<TInput2> fuzzrOfT2;
    private readonly Shrinker[] shrinkers;
    private readonly CheckrOf<Case>[] formatters;
    private readonly Func<TInput1, TInput2, Task<bool>> invariant;
    private readonly Func<TInput1, TInput2, int>? deliberation;
    private readonly int? deliberationTarget;
    private readonly bool useBuiltInReducers;

    internal TestrPropertyRunnerT2(
        FuzzrOf<TInput1> fuzzrOfT1,
        FuzzrOf<TInput2> fuzzrOfT2,
        Shrinker[] shrinkers,
        CheckrOf<Case>[] formatters,
        Func<TInput1, TInput2, Task<bool>> invariant,
        Func<TInput1, TInput2, int>? deliberation,
        int? deliberationTarget,
        string testName,
        bool useBuiltInReducers)
    {
        this.fuzzrOfT1 = fuzzrOfT1;
        this.fuzzrOfT2 = fuzzrOfT2;
        this.shrinkers = shrinkers;
        this.formatters = formatters;
        this.invariant = invariant;
        this.deliberation = deliberation;
        this.deliberationTarget = deliberationTarget;
        this.useBuiltInReducers = useBuiltInReducers;
        TestName = testName;
    }

    internal override string TestName { get; }

    internal override CheckrOf<Case> GetCheckr(
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
            () => deliberation is not null,
            Checkr.Deliberate(() => deliberation!(input.Item1, input.Item2)))
        from deliberateUntil in Checkr.When(
            () => deliberationTarget.HasValue,
            Trackr.DeliberateUntil(deliberationTarget.GetValueOrDefault()))
        from run in Checkr.ActCarefully("Run", () => invariant(input.Item1, input.Item2))
        from rethrow in Checkr.When(() => run.Threw, Checkr.Act("Rethrow", () => invariant(input.Item1, input.Item2)))
        from expectation in Checkr.ExpectWhen(TestName, () => !run.Threw, () => run.Value)
        select Case.Closed;

    internal override Func<CheckrConfig, CheckrConfig> GetConfig()
    {
        return a => a with
        {
            FileAs = fileName,
            Clerk = new PropertyClerk(),
            Custodian = custodian is null ? Custodian.Default : custodian,
            ShrinkMode = useBuiltInReducers ? a.ShrinkMode | ShrinkMode.Reduction : a.ShrinkMode,
            ReportMode = a.ReportMode
        };
    }
}
