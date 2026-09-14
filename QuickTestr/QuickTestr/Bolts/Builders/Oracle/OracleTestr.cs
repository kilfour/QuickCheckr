using QuickCheckr;
using QuickFuzzr;

namespace QuickTestr.Bolts.Builders.Oracle;

/// <summary>
/// Completes an oracle-based Testr after the expected behavior has been defined.
/// Use to provide the implementation that should match the trusted model.
/// </summary>
public class OracleTestr<T, TResult>(
    FuzzrOf<T> fuzzr,
    Shrinker[] shrinkers,
    CheckrOf<Case>[] formatters,
    Func<T, TResult> expected,
    Func<T, int>? deliberation,
    int? deliberationTarget,
    string testName,
    bool useBuiltInReducers)
{
    /// <summary>
    /// Defines the implementation that should match the expected result.
    /// Use to finish an oracle-style Testr and obtain a runnable comparison.
    /// </summary>
    public TestrOracleRunner<T, TResult> Actual(Func<T, TResult> actual)
        => new(fuzzr, shrinkers, formatters, expected, actual, deliberation, deliberationTarget, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous implementation that should match the expected result.
    /// Use to finish an oracle-style Testr when the implementation returns a task.
    /// </summary>
    public TestrOracleRunner<T, TResult> Actual(Func<T, Task<TResult>> actual)
        => new(
            fuzzr,
            shrinkers,
            formatters,
            expected,
            input => actual(input).GetAwaiter().GetResult(),
            deliberation,
            deliberationTarget,
            testName,
            useBuiltInReducers);
}
