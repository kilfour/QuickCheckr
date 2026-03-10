using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Experimental.K_Dsl.Dsl;

public class CombinedUpdateBuilder<TPoolElement1, TPoolElement2>(string label)
{
    public CombinedUpdateBuilder1<TPoolElement1, TPoolElement2> When(Func<TPoolElement1, TPoolElement2, bool> predicate)
        => new(label, predicate);
}

public class CombinedUpdateBuilder1<TPoolElement1, TPoolElement2>(
    string label,
    Func<TPoolElement1, TPoolElement2, bool> predicate)
{
    public CombinedUpdateBuilder2<TPoolElement1, TPoolElement2> Perform(Action<TPoolElement1, TPoolElement2> update)
        => new(label, predicate, update);
}

public class CombinedUpdateBuilder2<TPoolElement1, TPoolElement2>(
    string label,
    Func<TPoolElement1, TPoolElement2, bool> predicate,
    Action<TPoolElement1, TPoolElement2> update)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> Expect(
        params (string label, Func<TPoolElement1, TPoolElement2, bool> expectation)[] expectations) =>
        Trackr.PoolWhen(label, predicate,
            (element1, element2) =>
                from updated in Checkr.Act(label, () => update(element1.Value, element2.Value))
                from checks in Combine.Checkrs(expectations.Select(a =>
                    Checkr.Expect($"'{label}' {a.label}", () => a.expectation(element1.Value, element2.Value))))
                select Case.Closed);
}