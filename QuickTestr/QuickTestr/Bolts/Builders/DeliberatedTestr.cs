using QuickCheckr;
using QuickCheckr.Shrinking;
using QuickFuzzr;
using QuickTestr.Bolts.Builders.Oracle;
using QuickTestr.Bolts.Builders.Property;

namespace QuickTestr.Bolts.Builders;

/// <summary>
/// Completes a Testr after deliberation has been configured.
/// Use to finish the definition with either a property assertion or an oracle comparison.
/// </summary>
public sealed class DeliberatedTestr<T>
{
    private readonly FuzzrOf<T> fuzzr;
    private readonly Shrinker[] shrinkers;
    private readonly CheckrOf<Case>[] formatters;
    private readonly Func<T, int>? deliberation;
    private readonly int? deliberationTarget;
    private readonly string testName;
    private readonly bool useBuiltInReducers;

    internal DeliberatedTestr(
        FuzzrOf<T> fuzzr,
        Shrinker[] shrinkers,
        CheckrOf<Case>[] formatters,
        Func<T, int>? deliberation,
        int? deliberationTarget,
        string testName,
        bool useBuiltInReducers)
    {
        this.fuzzr = fuzzr;
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
    public TestrPropertyRunner<T> Assert(Func<T, bool> invariant)
        => new(fuzzr, shrinkers, formatters, input => Task.FromResult(invariant(input)), deliberation, deliberationTarget, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous property that must hold for generated inputs.
    /// Use when the invariant returns a task after deliberation has been configured.
    /// </summary>
    public TestrPropertyRunner<T> Assert(Func<T, Task<bool>> invariant)
        => new(
            fuzzr,
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
    public OracleTestr<T, TResult> Expected<TResult>(Func<T, TResult> expected)
        => new(fuzzr, shrinkers, formatters, input => Task.FromResult(expected(input)), deliberation, deliberationTarget, testName, useBuiltInReducers);

    /// <summary>
    /// Defines the asynchronous expected result for oracle-style testing.
    /// Use when the trusted model returns a task after deliberation has been configured.
    /// </summary>
    public OracleTestr<T, TResult> Expected<TResult>(Func<T, Task<TResult>> expected)
        => new(
            fuzzr,
            shrinkers,
            formatters,
            expected,
            deliberation,
            deliberationTarget,
            testName,
            useBuiltInReducers);


    /// <summary>Requires the exact exception type and optionally an ordinal message match computed from the current input.</summary>
    public TestrPropertyRunner<T> AssertThrewExactly<TException>(Action<T> action, Func<T, string>? message = null)
        where TException : Exception
        => new(fuzzr, shrinkers, formatters, _ => Task.FromResult(true), deliberation, deliberationTarget, testName, useBuiltInReducers)
        {
            Assertion = input => ExceptionProperty.Check<TException>(
                testName, Checkr.ActCarefully("Run", () => action(input)), message is null ? null : () => message(input))
        };

    /// <summary>Requires the exact exception type and optionally an ordinal message match computed from the current input.</summary>
    public TestrPropertyRunner<T> AssertThrewExactly<TException>(Func<T, Task> action, Func<T, string>? message = null)
        where TException : Exception
        => new(fuzzr, shrinkers, formatters, _ => Task.FromResult(true), deliberation, deliberationTarget, testName, useBuiltInReducers)
        {
            Assertion = input => ExceptionProperty.Check<TException>(
                testName, Checkr.ActCarefully("Run", () => action(input)), message is null ? null : () => message(input))
        };

    /// <summary>Requires the exact exception type and optionally an ordinal message match computed from the current input.</summary>
    public TestrPropertyRunner<T> AssertThrewExactly<TException>(Func<T, CancellationToken, Task> action, Func<T, string>? message = null)
        where TException : Exception
        => new(fuzzr, shrinkers, formatters, _ => Task.FromResult(true), deliberation, deliberationTarget, testName, useBuiltInReducers)
        {
            Assertion = input => ExceptionProperty.Check<TException>(
                testName, Checkr.ActCarefully("Run", token => action(input, token)), message is null ? null : () => message(input))
        };
}
