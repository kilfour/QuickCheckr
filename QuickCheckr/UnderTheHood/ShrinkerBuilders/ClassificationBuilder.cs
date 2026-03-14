using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

public class ClassificationBuilder<T>
{
    public ClassificationOption<T>[] Classify(params ClassificationOption<T>[] classifiers)
        => classifiers;
}
