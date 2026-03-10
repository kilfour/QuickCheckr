using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.K_Dsl.Dsl;

public class CreateBuilder<TPoolElement>(string label)
{
    public CreateBuilder1<TPoolElement> When(Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
        => new(label, predicate);
}

public class CreateBuilder1<TPoolElement>(string label, Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
{
    public CreateBuilder2<TPoolElement, TInput> With<TInput>(FuzzrOf<TInput> fuzzr) =>
        new(label, predicate, new InputInfo<TInput>(fuzzr, []));
}

public class CreateBuilder2<TPoolElement, TInput>(
    string label,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    InputInfo<TInput> inputInfo)
{
    public CreateBuilder3<TPoolElement, TInput> Perform(Func<TInput, TPoolElement> create) =>
        new(label, predicate, inputInfo, create);
}

public class CreateBuilder3<TPoolElement, TInput>(
    string label,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    InputInfo<TInput> inputInfo,
    Func<TInput, TPoolElement> create)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> Expect(
        params (string label, Func<TPoolElement, TInput, bool> expectation)[] expectations) =>
        Trackr.PoolWhen(predicate,
            from input in Checkr.Input($"'{label}' Input", inputInfo.Fuzzr, inputInfo.GetShrinkers())
            from created in Checkr.Act(label, () => create(input))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{label}' {a.label}", () => a.expectation(created, input))))
            from store in Trackr.ToPool($"'{label}' to Pool", created)
            select Case.Closed);
}
