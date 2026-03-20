using QuickCheckr;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.F_ModelBasedTesting;

public static class Check
{
    public static ModelBuilder<T> Model<T>(Func<T> model) => new(model);
}

public sealed class ModelBuilder<T>(Func<T> model)
{
    private readonly Func<T> model = model;
    public SutBuilder<T, U> Sut<U>(Func<U> sut) => new(model, sut);
}

public sealed class SutBuilder<T, U>(Func<T> model, Func<U> sut)
{
    private readonly Func<T> model = model;
    private readonly Func<U> sut = sut;

    private readonly List<Func<T, U, CheckrOf<Case>>> operations = [];

    public SutBuilder<T, U> Operation(string label, Action<T> modelOperation, Action<U> sutOperation)
    {
        operations.Add((m, s) => Checkr.Act(label, () => { modelOperation(m); sutOperation(s); }));
        return this;
    }

    public SutBuilder<T, U> Operation<V>(string label, FuzzrOf<V> fuzzr, Action<T, V> modelOperation, Action<U, V> sutOperation)
    {
        operations.Add((m, s) =>
            from input in Checkr.Input("Input", fuzzr)
            from act in Checkr.Act(label, () => { modelOperation(m, input); sutOperation(s, input); })
            select Case.Closed);

        return this;
    }

    private Func<T, U, string>? message;
    public SutBuilder<T, U> Report(Func<T, U, string> message)
    {
        this.message = message;
        return this;
    }

    public CheckrOf<Case> Observe(Func<T, U, bool> expectation)
    {
        if (operations.Count == 0)
            throw new InvalidOperationException("No operations defined. Add at least one .Operation(...) before calling Observe.");
        var checkr =
            from m in Trackr.Stashed(model)
            from s in Trackr.Stashed(sut)
            from _ in Checkr.OneOf([.. operations.Select(op => op(m, s))])
            from ok in Checkr.Capture(() => expectation(m, s))
            from observe in Checkr.Expect("Observe", () => ok)
            from trace in Checkr.TraceWhen("Observation", () => !ok && message != null, () => message!(m, s))
            select Case.Closed;
        return checkr;
    }
}