using QuickCheckr;
using QuickFuzzr;
using QuickTestr.Bolts.Builders.Oracle;
using QuickTestr.Bolts.Builders.Property;

namespace QuickTestr.Bolts.Builders;

/// <summary>
/// Configures how a generated input should be reduced, formatted, or evaluated.
/// Use to refine shrinking behavior before defining the actual assertion or oracle.
/// </summary>
public class InputTestr<T>(
    string testName,
    bool useBuiltInReducers,
    CheckrOf<Case>[] formatters,
    FuzzrOf<T> fuzzr,
    Shrinker[] shrinkers)
{
    /// <summary>
    /// Adds a deliberation score used to guide shrinking.
    /// Use when smaller or simpler counterexamples depend on a domain-specific notion of progress.
    /// </summary>
    public DeliberatedTestr<T> Deliberate(Func<T, int> deliberation)
        => new(fuzzr, shrinkers, formatters, deliberation, null, testName, useBuiltInReducers);

    /// <summary>
    /// Adds a deliberation score and target used to guide shrinking.
    /// Use when shrinking should move toward a specific domain-aware goal instead of just lower scores.
    /// </summary>
    public DeliberatedTestr<T> Deliberate(Func<T, int> deliberation, int deliberationTarget)
        => new(fuzzr, shrinkers, formatters, deliberation, deliberationTarget, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the property that must hold for generated inputs.
    /// Use for direct property-based testing where success is a boolean invariant.
    /// </summary>
    public TestrPropertyRunner<T> Assert(Func<T, bool> invariant)
        => new(fuzzr, shrinkers, formatters, invariant, null, null, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous property that must hold for generated inputs.
    /// Use for direct property-based testing when the invariant returns a task.
    /// </summary>
    public TestrPropertyRunner<T> Assert(Func<T, Task<bool>> invariant)
        => new(
            fuzzr,
            shrinkers,
            formatters,
            input => invariant(input).GetAwaiter().GetResult(),
            null,
            null,
            testName,
            useBuiltInReducers);

    /// <summary>
    /// Defines the expected result for oracle-style testing.
    /// Use when you want to compare a trusted model against another implementation.
    /// </summary>
    public OracleTestr<T, TResult> Expected<TResult>(Func<T, TResult> expected)
        => new(fuzzr, shrinkers, formatters, expected, null, null, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous expected result for oracle-style testing.
    /// Use when the trusted model returns a task.
    /// </summary>
    public OracleTestr<T, TResult> Expected<TResult>(Func<T, Task<TResult>> expected)
        => new(
            fuzzr,
            shrinkers,
            formatters,
            input => expected(input).GetAwaiter().GetResult(),
            null,
            null,
            testName,
            useBuiltInReducers);
}
