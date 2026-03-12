using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Bolts;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl.OperationBuilders.Bolts;

public interface IOperationExample { public CheckrOf<Case> GetCheckr(); }

public record OperationExample<TPoolElement, TInput>(
    string Label,
    TInput Input,
    Func<TInput, TPoolElement> Create,
    Func<DelayedResult<TPoolElement>, bool> Expectation) : IOperationExample
{
    public CheckrOf<Case> GetCheckr() =>
        from flag in Trackr.Stashed(Label, () => new Cell<bool>(true))
        from maybe in Checkr.When(() => flag.Value,
            from input in Checkr.Input(Label, Fuzzr.Constant(Input))
            from created in Checkr.ActCarefully(Label, () => Create(input))
            from checks in Checkr.Expect(Label, () => Expectation(created))
            from flip in Checkr.Perform(() => flag.Value = !flag.Value)
            select Case.Closed)
        select Case.Closed;
}