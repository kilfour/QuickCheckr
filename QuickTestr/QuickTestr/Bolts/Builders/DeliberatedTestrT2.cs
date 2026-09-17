using QuickCheckr;
using QuickFuzzr;
using QuickTestr.Bolts.Builders.Oracle;
using QuickTestr.Bolts.Builders.Property;

namespace QuickTestr.Bolts.Builders;

/// <summary>
/// Completes a Testr after deliberation has been configured.
/// Use to finish the definition with either a property assertion or an oracle comparison.
/// </summary>
public sealed class DeliberatedTestrT2<TInput1, TInput2>
{
    private readonly FuzzrOf<TInput1> fuzzrOfT1;
    private readonly FuzzrOf<TInput2> fuzzrOfT2;
    private readonly Shrinker[] shrinkers;
    private readonly CheckrOf<Case>[] formatters;
    private readonly Func<TInput1, TInput2, int>? deliberation;
    private readonly int? deliberationTarget;
    private readonly string testName;
    private readonly bool useBuiltInReducers;

    internal DeliberatedTestrT2(
        FuzzrOf<TInput1> fuzzrOfT1,
        FuzzrOf<TInput2> fuzzrOfT2,
        Shrinker[] shrinkers,
        CheckrOf<Case>[] formatters,
        Func<TInput1, TInput2, int>? deliberation,
        int? deliberationTarget,
        string testName,
        bool useBuiltInReducers)
    {
        this.fuzzrOfT1 = fuzzrOfT1;
        this.fuzzrOfT2 = fuzzrOfT2;
        this.shrinkers = shrinkers;
        this.formatters = formatters;
        this.deliberation = deliberation;
        this.deliberationTarget = deliberationTarget;
        this.testName = testName;
        this.useBuiltInReducers = useBuiltInReducers;
    }

    /// <summary>
    /// Defines the property that must hold for generated inputs.
    /// Use for direct property-based testing after adding deliberation settings.
    /// </summary>
    public TestrPropertyRunnerT2<TInput1, TInput2> Assert(Func<TInput1, TInput2, bool> invariant)
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, (input1, input2) => Task.FromResult(invariant(input1, input2)), deliberation, deliberationTarget, testName, useBuiltInReducers);

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
            invariant,
            deliberation,
            deliberationTarget,
            testName,
            useBuiltInReducers);

    /// <summary>
    /// Defines the expected result for oracle-style testing.
    /// Use when you want to compare a trusted model against another implementation after adding deliberation settings.
    /// </summary>
    public OracleTestrT2<TInput1, TInput2, TResult> Expected<TResult>(Func<TInput1, TInput2, TResult> expected)
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, (input1, input2) => Task.FromResult(expected(input1, input2)), deliberation, deliberationTarget, testName, useBuiltInReducers);

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
            expected,
            deliberation,
            deliberationTarget,
            testName,
            useBuiltInReducers);


    /// <summary>Requires the exact exception type and optionally an ordinal message match computed from the current input.</summary>
    public TestrPropertyRunnerT2<TInput1, TInput2> AssertThrewExactly<TException>(Action<TInput1, TInput2> action, Func<TInput1, TInput2, string>? message = null)
        where TException : Exception
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, (_, _) => Task.FromResult(true), deliberation, deliberationTarget, testName, useBuiltInReducers)
        {
            Assertion = input => ExceptionProperty.Check<TException>(
                testName, Checkr.ActCarefully("Run", () => action(input.Item1, input.Item2)), message is null ? null : () => message(input.Item1, input.Item2))
        };

    /// <summary>Requires the exact exception type and optionally an ordinal message match computed from the current input.</summary>
    public TestrPropertyRunnerT2<TInput1, TInput2> AssertThrewExactly<TException>(Func<TInput1, TInput2, Task> action, Func<TInput1, TInput2, string>? message = null)
        where TException : Exception
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, (_, _) => Task.FromResult(true), deliberation, deliberationTarget, testName, useBuiltInReducers)
        {
            Assertion = input => ExceptionProperty.Check<TException>(
                testName, Checkr.ActCarefully("Run", () => action(input.Item1, input.Item2)), message is null ? null : () => message(input.Item1, input.Item2))
        };

    /// <summary>Requires the exact exception type and optionally an ordinal message match computed from the current input.</summary>
    public TestrPropertyRunnerT2<TInput1, TInput2> AssertThrewExactly<TException>(Func<TInput1, TInput2, CancellationToken, Task> action, Func<TInput1, TInput2, string>? message = null)
        where TException : Exception
        => new(fuzzrOfT1, fuzzrOfT2, shrinkers, formatters, (_, _) => Task.FromResult(true), deliberation, deliberationTarget, testName, useBuiltInReducers)
        {
            Assertion = input => ExceptionProperty.Check<TException>(
                testName, Checkr.ActCarefully("Run", token => action(input.Item1, input.Item2, token)), message is null ? null : () => message(input.Item1, input.Item2))
        };
}
