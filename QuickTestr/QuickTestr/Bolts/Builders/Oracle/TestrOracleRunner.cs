using QuickCheckr;
using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Bolts.Builders.Oracle;

/// <summary>
/// Runs an oracle-based Testr by comparing expected and actual behavior.
/// Use for Testrs defined through Expected and Actual.
/// </summary>
public sealed class TestrOracleRunner<TInput, TResult> : TestrRunner<TInput>
{
    private readonly FuzzrOf<TInput> fuzzr;
    private readonly Shrinker[] shrinkers;
    private readonly CheckrOf<Case>[] formatters;
    private readonly Func<TInput, TResult> Expected;
    private readonly Func<TInput, TResult> Actual;
    private readonly Func<TInput, int>? Deliberation;
    private readonly int? DeliberationTarget;
    private readonly bool UseBuiltInReducers;

    internal TestrOracleRunner(
        FuzzrOf<TInput> fuzzr,
        Shrinker[] shrinkers,
        CheckrOf<Case>[] formatters,
        Func<TInput, TResult> expected,
        Func<TInput, TResult> actual,
        Func<TInput, int>? deliberation,
        int? deliberationTarget,
        string testName,
        bool useBuiltInReducers)
    {
        this.fuzzr = fuzzr;
        this.shrinkers = shrinkers;
        this.formatters = formatters;
        Expected = expected;
        Actual = actual;
        Deliberation = deliberation;
        DeliberationTarget = deliberationTarget;
        UseBuiltInReducers = useBuiltInReducers;
        TestName = testName;
    }

    internal override string TestName { get; }

    internal override CheckrOf<Case> GetCheckr(
        Func<TInput, object>? distinguish = null,
        Func<TInput, bool>? rejectCaseWhen = null) =>
        from showr in Showr.ForInput()
        from format in Combine.Checkrs(formatters)
        from input in Checkr.Input("Input", fuzzr, shrinkers)
        from noteInput in Autopsy.Note("Input", () => input)
        from distinguishCase in Checkr.When(
            () => distinguish is not null,
            Checkr.Distinguish(() => distinguish!(input)))
        from rejectCase in Checkr.When(
            () => rejectCaseWhen is not null,
            Checkr.RejectCaseWhen(() => rejectCaseWhen!(input)))
        from deliberate in Checkr.When(
            () => Deliberation is not null,
            Checkr.Deliberate(() => Deliberation!(input)))
        from deliberateUntil in Checkr.When(
            () => DeliberationTarget.HasValue,
            Trackr.DeliberateUntil(DeliberationTarget.GetValueOrDefault()))
        from expected in Checkr.ActCarefully("Expected", () => Expected(input))
        from actual in Checkr.ActCarefully("Actual", () => Actual(input))
        from traceExpected in Trace("Expected", expected)
        from noteExpected in Note("Expected", expected)
        from traceActual in Trace("Actual  ", actual)
        from noteActual in Note("Actual  ", actual)
        from expectation in Checkr.Expect(TestName, () => CheckResults(expected, actual))
        select Case.Closed;

    private static CheckrOf<Case> Trace(string label, DelayedResult<TResult> result) =>
        from traceValue in Checkr.TraceWhen(label, () => !result.Threw && result.HasValue, () => result.Value)
        from traceValueNull in Checkr.TraceWhen(label, () => !result.Threw && !result.HasValue, () => "null")
        from traceException in Checkr.TraceWhen(label, () => result.Threw, () => GetExceptionReport(result.Exception!))
        select Case.Closed;

    private static CheckrOf<Case> Note(string label, DelayedResult<TResult> result) =>
        from noteValue in Checkr.When(() => !result.Threw && result.HasValue, Autopsy.Note(label, () => result.Value))
        from noteValueNull in Checkr.When(() => !result.Threw && !result.HasValue, Autopsy.Note(label, () => "null"))
        from noteException in Checkr.When(() => result.Threw, Autopsy.Note(label, () => GetExceptionReport(result.Exception!)))
        select Case.Closed;

    private static string GetExceptionReport(Exception exception)
        => $"{exception.GetType().Name}: {exception.Message}";

    private static bool CheckResults(DelayedResult<TResult> expected, DelayedResult<TResult> actual)
    {
        if (!expected.Threw && !actual.Threw)
        {
            if (expected.HasValue != actual.HasValue)
                return false;
            if (!expected.HasValue)
                return true;
            return Equals(expected.Value, actual.Value);
        }
        if (expected.Threw && actual.Threw)
            return ExceptionIs.Equivalent(expected.Exception, actual.Exception);
        return false;
    }

    internal override Func<CheckrConfig, CheckrConfig> GetConfig()
    {
        return a => a with
        {
            FileAs = fileName,
            Clerk = new OracleClerk(),
            Custodian = custodian is null ? Custodian.Default : custodian,
            ShrinkMode = UseBuiltInReducers ? a.ShrinkMode | ShrinkMode.Reduction : a.ShrinkMode,
            ReportMode = a.ReportMode | ReportMode.FinalTrace
        };
    }
}
