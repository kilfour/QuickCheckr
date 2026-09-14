using QuickCheckr;

namespace QuickTestr.Bolts.Builders.ModelBased;

/// <summary>
/// Starts a model-based Testr from a trusted model factory.
/// Use when defining a comparison between a reference implementation and a system under test.
/// </summary>
public sealed class WithModel<T>
{
    private readonly string testName;
    private readonly bool useBuiltInReducers;
    private readonly CheckrOf<Case>[] formatters;
    private readonly Func<T> model;

    internal WithModel(
        string testName,
        bool useBuiltInReducers,
        CheckrOf<Case>[] formatters,
        Func<T> model)
    {
        this.testName = testName;
        this.useBuiltInReducers = useBuiltInReducers;
        this.formatters = formatters;
        this.model = model;
    }

    /// <summary>
    /// Supplies the system under test for model-based comparisons.
    /// Use when moving from the reference model to operation and observation setup.
    /// </summary>
    public WithSut<T, U> Sut<U>(Func<U> sut) => new(testName, useBuiltInReducers, formatters, model, sut);
}
