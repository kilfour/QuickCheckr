using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static class Shrink
{
    public static Shrinker OnType<T>(Func<IClassificationOption> shrinkOption)
        => new(() => [shrinkOption()], []);

    public static Shrinker OnType<T>(Func<IClassificationOption[]> shrinkOptions)
        => new(shrinkOptions, []);

    public static Shrinker OnType<T>(Func<Reducer<T>> reducer)
        => new(() => [], [new TypeReducerOption<T>(reducer)]);
}
