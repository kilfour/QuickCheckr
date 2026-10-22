using QuickCheckr;
using QuickCheckr.Shrinking;
using QuickFuzzr;

namespace QuickTestr.Bolts.Builders.Oracle;

/// <summary>
/// Completes an oracle-based Testr after the expected behavior has been defined.
/// Use to provide the implementation that should match the trusted model.
/// </summary>
public sealed class OracleTestrT2<TInput1, TInput2, TResult>
{
    private readonly FuzzrOf<TInput1> fuzzrOfT1;
    private readonly FuzzrOf<TInput2> fuzzrOfT2;
    private readonly Shrinker[] shrinkers;
    private readonly CheckrOf<Case>[] formatters;
    private readonly Func<TInput1, TInput2, Task<TResult>> expected;
    private readonly Func<TInput1, TInput2, int>? deliberation;
    private readonly int? deliberationTarget;
    private readonly string testName;
    private readonly bool useBuiltInReducers;

    internal OracleTestrT2(
        FuzzrOf<TInput1> fuzzrOfT1,
        FuzzrOf<TInput2> fuzzrOfT2,
        Shrinker[] shrinkers,
        CheckrOf<Case>[] formatters,
        Func<TInput1, TInput2, Task<TResult>> expected,
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
        this.deliberation = deliberation;
        this.deliberationTarget = deliberationTarget;
        this.testName = testName;
        this.useBuiltInReducers = useBuiltInReducers;
    }

    /// <summary>
    /// Defines the implementation that should match the expected result.
    /// Use to finish an oracle-style Testr and obtain a runnable comparison.
    /// </summary>
    public TestrOracleRunnerT2<TInput1, TInput2, TResult> Actual(Func<TInput1, TInput2, TResult> actual)
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, expected, (input1, input2) => Task.FromResult(actual(input1, input2)), deliberation, deliberationTarget, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous implementation that should match the expected result.
    /// Use to finish an oracle-style Testr when the implementation returns a task.
    /// </summary>
    public TestrOracleRunnerT2<TInput1, TInput2, TResult> Actual(
        Func<TInput1, TInput2, Task<TResult>> actual)
        => new(
            fuzzrOfT1,
            fuzzrOfT2,
            shrinkers,
            formatters,
            expected,
            actual,
            deliberation,
            deliberationTarget,
            testName,
            useBuiltInReducers);
}
