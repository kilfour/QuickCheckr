using QuickCheckr;

namespace QuickTestr.Bolts.Builders.ModelBased;

/// <summary>
/// Starts a model-based Testr from a trusted model factory.
/// Use when defining a comparison between a reference implementation and a system under test.
/// </summary>
public class WithModel<T>(string testName, bool useBuiltInReducers, CheckrOf<Case>[] formatters, Func<T> model)
{
    private readonly Func<T> model = model;

    /// <summary>
    /// Supplies the system under test for model-based comparisons.
    /// Use when moving from the reference model to operation and observation setup.
    /// </summary>
    public WithSut<T, U> Sut<U>(Func<U> sut) => new(testName, useBuiltInReducers, formatters, model, sut);
}
