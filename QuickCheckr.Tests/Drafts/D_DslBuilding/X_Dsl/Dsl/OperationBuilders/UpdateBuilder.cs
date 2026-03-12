using QuickCheckr;
using QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl.OperationBuilders.Bolts;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl.OperationBuilders;


public class UpdateBuilder<TPoolElement>(string label)
{
    public UpdateBuilder1<TPoolElement> When(Func<IReadOnlyCollection<TPoolElement>, bool> predicate) =>
        new(label, new PoolCondition<TPoolElement>(predicate, null));
}

public class UpdateBuilder1<TPoolElement>(string label, PoolCondition<TPoolElement> poolCondition)
{
    public UpdateBuilder2<TPoolElement, TInput> With<TInput>(FuzzrOf<TInput> fuzzr) =>
        new(label, poolCondition, new InputInfo<TPoolElement, TInput>(fuzzr, [], null));
}

public class UpdateBuilder2<TPoolElement, TInput>(
    string label,
    PoolCondition<TPoolElement> poolCondition,
    InputInfo<TPoolElement, TInput> inputInfo)
{
    public UpdateBuilder3<TPoolElement, TInput> Perform(Action<TPoolElement, TInput> update) =>
        new(label, poolCondition, inputInfo, update);
}

public class UpdateBuilder3<TPoolElement, TInput>(
    string label,
    PoolCondition<TPoolElement> poolCondition,
    InputInfo<TPoolElement, TInput> inputInfo,
    Action<TPoolElement, TInput> update)
{
    public OperationSpecification Expect(
        params (string label, Func<TPoolElement, TInput, bool> expectation)[] expectations) =>
        new(poolCondition.GetCheckr(label, element =>
            from input in Checkr.Input($"'{label}' Input", inputInfo.Fuzzr, inputInfo.GetShrinkers(element.Value))
            from updated in Checkr.Act(label, () => update(element.Value, input))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{label}' {a.label}", () => a.expectation(element.Value, input))))
            select Case.Closed));
}