using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickFuzzr;

namespace QuickCheckr.Invariants;


public static class Testr
{
    public static TestrBuilder0 Named(string name) => new(name);
    public static TestrBuilder1<T> For<T>(FuzzrOf<T> fuzzr, params Shrinker[] shrinkers)
        => new(fuzzr, shrinkers);
}

public class TestrBuilder0(string testName)
{
    public TestrBuilder1<T> For<T>(FuzzrOf<T> fuzzr, params Shrinker[] shrinkers)
        => new(fuzzr, shrinkers, testName);
}

public class TestrBuilder1<T>(FuzzrOf<T> fuzzr, Shrinker[] shrinkers, string testName = "Invariant")
{
    private bool useBuiltInReducers = true;
    private CheckrOf<Case>[] formatters = [];
    public TestrBuilder1<T> DisableValueReduction() { useBuiltInReducers = false; return this; }
    public TestrBuilder1<T> Format(CheckrOf<Case>[] formatters) { this.formatters = formatters; return this; }
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation)
        => new(fuzzr, shrinkers, formatters, deliberation, null, testName, useBuiltInReducers);
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation, int deliberationTarget)
        => new(fuzzr, shrinkers, formatters, deliberation, deliberationTarget, testName, useBuiltInReducers);
    public TestrRunner<T> Assert(Func<T, bool> invariant)
        => new(fuzzr, shrinkers, formatters, invariant, null, null, testName, useBuiltInReducers);
}

public class TestrBuilder2<T>(
    FuzzrOf<T> fuzzr,
    Shrinker[] shrinkers,
    CheckrOf<Case>[] formatters,
    Func<T, int>? deliberation,
    int? deliberationTarget,
    string testName,
    bool useBuiltInReducers)
{
    public TestrRunner<T> Assert(Func<T, bool> invariant)
        => new(fuzzr, shrinkers, formatters, invariant, deliberation, deliberationTarget, testName, useBuiltInReducers);
}
