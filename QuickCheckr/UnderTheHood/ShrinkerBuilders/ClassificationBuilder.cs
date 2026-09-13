using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

/// <summary>
/// Selects how inputs of type <typeparamref name="T"/> are classified for shrinking.
/// </summary>
public sealed class ClassificationBuilder<T>
{
    public ClassificationOption<T>[] Classify(params ClassificationOption<T>[] classifiers)
        => classifiers;
}
