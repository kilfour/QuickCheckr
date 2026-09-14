using QuickCheckr;
using QuickFuzzr;
using QuickTestr.Bolts.Builders.Oracle;
using QuickTestr.Bolts.Builders.Property;

namespace QuickTestr.Bolts.Builders;

/// <summary>
/// Completes a Testr after deliberation has been configured.
/// Use to finish the definition with either a property assertion or an oracle comparison.
/// </summary>
public class DeliberatedTestrT2<TInput1, TInput2>(
    FuzzrOf<TInput1> fuzzrOfT1,
    FuzzrOf<TInput2> fuzzrOfT2,
    Shrinker[] shrinkers,
    CheckrOf<Case>[] formatters,
    Func<TInput1, TInput2, int>? deliberation,
    int? deliberationTarget,
    string testName,
    bool useBuiltInReducers)
{
    /// <summary>
    /// Defines the property that must hold for generated inputs.
    /// Use for direct property-based testing after adding deliberation settings.
    /// </summary>
    public TestrPropertyRunnerT2<TInput1, TInput2> Assert(Func<TInput1, TInput2, bool> invariant)
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, invariant, deliberation, deliberationTarget, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous property that must hold for generated inputs.
    /// Use when the invariant returns a task after deliberation has been configured.
    /// </summary>
    public TestrPropertyRunnerT2<TInput1, TInput2> Assert(
        Func<TInput1, TInput2, Task<bool>> invariant)
        => new(
            fuzzrOfT1,
            fuzzrOfT2,
            shrinkers,
            formatters,
            (input1, input2) => invariant(input1, input2).GetAwaiter().GetResult(),
            deliberation,
            deliberationTarget,
            testName,
            useBuiltInReducers);

    /// <summary>
    /// Defines the expected result for oracle-style testing.
    /// Use when you want to compare a trusted model against another implementation after adding deliberation settings.
    /// </summary>
    public OracleTestrT2<TInput1, TInput2, TResult> Expected<TResult>(Func<TInput1, TInput2, TResult> expected)
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, expected, deliberation, deliberationTarget, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous expected result for oracle-style testing.
    /// Use when the trusted model returns a task after deliberation has been configured.
    /// </summary>
    public OracleTestrT2<TInput1, TInput2, TResult> Expected<TResult>(
        Func<TInput1, TInput2, Task<TResult>> expected)
        => new(
            fuzzrOfT1,
            fuzzrOfT2,
            shrinkers,
            formatters,
            (input1, input2) => expected(input1, input2).GetAwaiter().GetResult(),
            deliberation,
            deliberationTarget,
            testName,
            useBuiltInReducers);
}
