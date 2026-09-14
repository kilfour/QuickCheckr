using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Bolts.Builders.Oracle;

/// <summary>
/// Runs an oracle-based Testr by comparing expected and actual behavior.
/// Use for Testrs defined through Expected and Actual.
/// </summary>
public sealed class TestrOracleRunnerT2<TInput1, TInput2, TResult> : TestrRunner<(TInput1, TInput2)>
{
    private readonly FuzzrOf<TInput1> fuzzrOfT1;
    private readonly FuzzrOf<TInput2> fuzzrOfT2;
    private readonly Shrinker[] shrinkers;
    private readonly CheckrOf<Case>[] formatters;
    private readonly Func<TInput1, TInput2, TResult> expected;
    private readonly Func<TInput1, TInput2, TResult> actual;
    private readonly Func<TInput1, TInput2, int>? deliberation;
    private readonly int? deliberationTarget;
    private readonly bool useBuiltInReducers;

    internal TestrOracleRunnerT2(
        FuzzrOf<TInput1> fuzzrOfT1,
        FuzzrOf<TInput2> fuzzrOfT2,
        Shrinker[] shrinkers,
        CheckrOf<Case>[] formatters,
        Func<TInput1, TInput2, TResult> expected,
        Func<TInput1, TInput2, TResult> actual,
        Func<TInput1, TInput2, int>? deliberation,
        int? deliberationTarget,
        string testName,
        bool useBuiltInReducers)
    {
        this.fuzzrOfT1 = fuzzrOfT1;
        this.fuzzrOfT2 = fuzzrOfT2;
        this.shrinkers = shrinkers;
        this.formatters = formatters;
        this.expected = expected;
        this.actual = actual;
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
        from expected in Checkr.ActCarefully("Expected", () => expected(input.Item1, input.Item2))
        from actual in Checkr.ActCarefully("Actual", () => actual(input.Item1, input.Item2))
        from traceExpectedValue in Checkr.TraceWhen("Expected", () => !expected.Threw && expected.HasValue, () => expected.Value)
        from traceExpectedValueNull in Checkr.TraceWhen("Expected", () => !expected.Threw && !expected.HasValue, () => "null")
        from traceExpectedException in Checkr.TraceWhen("Expected", () => expected.Threw, () => GetExceptionReport(expected.Exception!))
        from traceActualValue in Checkr.TraceWhen("Actual  ", () => !actual.Threw && actual.HasValue, () => actual.Value)
        from traceActualValueNull in Checkr.TraceWhen("Actual  ", () => !actual.Threw && !actual.HasValue, () => "null")
        from traceActualException in Checkr.TraceWhen("Actual  ", () => actual.Threw, () => GetExceptionReport(actual.Exception!))
        from expectation in Checkr.Expect(TestName, () => CheckResults(expected, actual))
        select Case.Closed;

    private static string GetExceptionReport(Exception exception)
    {
        return $"{exception!.GetType().Name}: {exception.Message}";
    }

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
            ShrinkMode = useBuiltInReducers ? a.ShrinkMode | ShrinkMode.Reduction : a.ShrinkMode,
            ReportMode = a.ReportMode | ReportMode.FinalTrace
        };
    }
}
