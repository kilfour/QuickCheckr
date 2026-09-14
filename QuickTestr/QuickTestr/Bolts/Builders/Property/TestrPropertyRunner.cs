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
public class TestrPropertyRunner<TInput>(
    FuzzrOf<TInput> fuzzr,
    Shrinker[] shrinkers,
    CheckrOf<Case>[] formatters,
    Func<TInput, bool> invariant,
    Func<TInput, int>? deliberation,
    int? deliberationTarget,
    string testName,
    bool useBuiltInReducers) : TestrRunner<TInput>
{
    protected override string TestName { get; } = testName;

    protected override CheckrOf<Case> GetCheckr(
        Func<TInput, object>? distinguish = null,
        Func<TInput, bool>? rejectCaseWhen = null) =>
        from showr in Showr.ForInput()
        from format in Combine.Checkrs(formatters)
        from input in Checkr.Input("Input", fuzzr, shrinkers)
        from distinguishCase in Checkr.When(
            () => distinguish is not null,
            Checkr.Distinguish(() => distinguish!(input)))
        from rejectCase in Checkr.When(
            () => rejectCaseWhen is not null,
            Checkr.RejectCaseWhen(() => rejectCaseWhen!(input)))
        from deliberate in Checkr.When(
            () => deliberation is not null,
            Checkr.Deliberate(() => deliberation!(input)))
        from deliberateUntil in Checkr.When(
            () => deliberationTarget.HasValue,
            Trackr.DeliberateUntil(deliberationTarget.GetValueOrDefault()))
        from run in Checkr.ActCarefully("Run", () => invariant(input))
        from rethrow in Checkr.When(() => run.Threw, Checkr.Act("Rethrow", () => invariant(input)))
        from expectation in Checkr.ExpectWhen(TestName, () => !run.Threw, () => run.Value)
        select Case.Closed;

    protected override Func<CheckrConfig, CheckrConfig> GetConfig()
    {
        return a => a with
        {
            FileAs = fileName,
            Custodian = custodian is null ? Custodian.Default : custodian,
            Clerk = new PropertyClerk(),
            ShrinkMode = useBuiltInReducers ? a.ShrinkMode | ShrinkMode.Reduction : a.ShrinkMode
        };
    }
}
