using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;

public record InputInfo<TInput>(
    FuzzrOf<TInput> Fuzzr,
    Shrinker[] Shrinkers)
{
    public Shrinker[] GetShrinkers() => Shrinkers;
};
public record InputInfo<TPoolElement, TInput>(
    FuzzrOf<TInput> Fuzzr,
    Shrinker[] Shrinkers,
    Func<TPoolElement, Shrinker>? ShrinkerFactory)
{
    public Shrinker[] GetShrinkers(TPoolElement element) =>
        ShrinkerFactory == null ? Shrinkers : [.. Shrinkers, ShrinkerFactory(element)];
};
