using QuickCheckr;
using QuickFuzzr;
using QuickTestr.Bolts.Builders.ModelBased;

namespace QuickTestr.Bolts.Builders;

/// <summary>
/// Starts the fluent configuration of a Testr.
/// Use to opt into case-file storage or move on to input generation.
/// </summary>
public class NamedTestr(string testName)
{
    private bool useBuiltInReducers = true;
    private CheckrOf<Case>[] formatters = [];

    /// <summary>
    /// Disables the built-in value reduction step during shrinking.
    /// Use when structural shrinking is enough or value reduction pushes examples the wrong way.
    /// </summary>
    public NamedTestr DisableValueReduction() { useBuiltInReducers = false; return this; }

    /// <summary>
    /// Adds custom formatters to the report for the generated input.
    /// Use when the default rendering does not explain the failing example clearly enough.
    /// </summary>
    public NamedTestr Format(params CheckrOf<Case>[] formatters) { this.formatters = formatters; return this; }

    /// <summary>
    /// Selects the input generator and optional custom shrinkers for this Testr.
    /// Use to define the values QuickTestr should explore and how they should shrink.
    /// </summary>
    public InputTestr<T> For<T>(FuzzrOf<T> fuzzr, params Shrinker[] shrinkers)
        => new(testName, useBuiltInReducers, formatters, fuzzr, shrinkers);

    /// <summary>
    /// Selects two input generators and optional custom shrinkers for this Testr.
    /// Use to define pairs of values QuickTestr should explore together and how they should shrink.
    /// </summary>
    public InputTestrT2<T1, T2> For<T1, T2>(FuzzrOf<T1> fuzzrOfT1, FuzzrOf<T2> fuzzrOfT2, params Shrinker[] shrinkers)
        => new(testName, useBuiltInReducers, formatters, fuzzrOfT1, fuzzrOfT2, shrinkers);

    /// <summary>
    /// Starts a model-based Testr from a trusted model instance.
    /// Use when you want to compare a stateful system under test against a reference implementation.
    /// </summary>
    public WithModel<T> Model<T>(Func<T> model) => new(testName, useBuiltInReducers, formatters, model);
}
