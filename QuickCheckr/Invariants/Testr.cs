using QuickFuzzr;

namespace QuickCheckr.Invariants;


public static class Testr
{
    public static TestrBuilder0 Named(string name) => new(name);
    public static TestrBuilder1<T> For<T>(FuzzrOf<T> fuzzr) => new(fuzzr);
}

public class TestrBuilder0(string testName)
{
    public TestrBuilder1<T> For<T>(FuzzrOf<T> fuzzr) => new(fuzzr, testName);
}

public class TestrBuilder1<T>(FuzzrOf<T> fuzzr, string testName = "Invariant")
{
    private bool useBuiltInReducers = true;
    public TestrBuilder1<T> DisableValueReduction() { useBuiltInReducers = false; return this; }
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation)
        => new(fuzzr, deliberation, null, testName, useBuiltInReducers);
    public TestrBuilder2<T> Deliberate(Func<T, int> deliberation, int deliberationTarget)
        => new(fuzzr, deliberation, deliberationTarget, testName, useBuiltInReducers);
    public TestrRunner<T> Assert(Func<T, bool> invariant) =>
        new(fuzzr, invariant, null, null, testName, useBuiltInReducers);
}

public class TestrBuilder2<T>(
    FuzzrOf<T> fuzzr,
    Func<T, int>? deliberation,
    int? deliberationTarget,
    string testName,
    bool useBuiltInReducers)
{
    public TestrRunner<T> Assert(Func<T, bool> invariant) =>
        new(fuzzr, invariant, deliberation, deliberationTarget, testName, useBuiltInReducers);
}
