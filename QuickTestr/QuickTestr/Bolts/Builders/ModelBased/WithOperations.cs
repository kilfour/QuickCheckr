using System.Diagnostics;
using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Bolts.Builders.ModelBased;

/// <summary>
/// Completes a model-based Testr after operations and an initial observation are defined.
/// Use when adding more observations or running the configured model-based check.
/// </summary>
public class WithOperations<T, U>(
    string testName,
    bool useBuiltInReducers,
    CheckrOf<Case>[] formatters,
    Func<T> model,
    Func<U> sut,
    bool verifyReturnValues,
    List<Func<bool, T, U, CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)>>> operations,
    Observation<T, U> observation)
{
    private string fileName = string.Empty;
    private ICustodian? custodian;

    /// <summary>
    /// Persists case files for this Testr under its test name.
    /// Use when you want to inspect or clean up stored cases later through the vault workflow.
    /// </summary>
    public WithOperations<T, U> StoreCaseFiles(ICustodian? custodian = null)
    {
        fileName = TestName;
        this.custodian = custodian;
        return this;
    }

    /// <summary>
    /// Gets the display name of this Testr.
    /// Use when you need the configured name for reporting or storage.
    /// </summary>
    public string TestName { get; } = testName;

    /// <summary>
    /// Adds another observation to the model-based Testr.
    /// Use when several invariants should hold after each generated operation.
    /// </summary>
    public WithOperations<T, U> Observe(string label, Func<T, U, bool> observe)
    {
        observations.Add(Observation.From(label, observe));
        return this;
    }

    /// <summary>
    /// Adds another observation with custom trace output to the model-based Testr.
    /// Use when a failing invariant needs targeted diagnostic values in the report.
    /// </summary>
    public WithOperations<T, U> Observe(string label, Func<T, U, bool> observe, Func<ITracer<T, U>, ITracer<T, U>> trace)
    {
        observations.Add(Observation.From(label, observe, ((Tracer<T, U>)trace(new Tracer<T, U>())).TraceCheckr));
        return this;
    }

    /// <summary>
    /// Runs the model-based Testr using the default execution settings.
    /// Use for the normal execution path when you do not need explicit run control.
    /// </summary>
    [StackTraceHidden]
    public WithOperations<T, U> Run()
    {
        GetCheckr().Configure(GetConfig()).Run(10.Runs(), 50.ExecutionsPerRun());
        return this;
    }


    /// <summary>
    /// Runs the model-based Testr using the specified run and execution counts.
    /// Use when you want to control how much stateful exploration is performed.
    /// </summary>
    [StackTraceHidden]
    public WithOperations<T, U> Run(RunCount runs, ExecutionCount executionsPerRun)
    {
        GetCheckr().Configure(GetConfig()).Run(runs, executionsPerRun);
        return this;
    }


    /// <summary>
    /// Runs the model-based Testr using the specified seed.
    /// Use when you want to reproduce a known stateful execution path.
    /// </summary>
    [StackTraceHidden]
    public WithOperations<T, U> Run(int seed, ExecutionCount executionsPerRun)
    {
        GetCheckr().Configure(GetConfig()).Run(seed, executionsPerRun);
        return this;
    }


    private Func<CheckrConfig, CheckrConfig> GetConfig()
    {
        return a => a with
        {
            FileAs = fileName,
            Clerk = new ModelClerk(),
            Custodian = custodian is null ? Custodian.Default : custodian,
            // DeliberationPolicy = Deliberation == null ? null :
            //     a => a.InputsNamed<TInput>("Input", a => Deliberation(a)),
            // DeliberationTarget = DeliberationTarget == null ? null : DeliberationTarget,
            ShrinkMode = useBuiltInReducers ? a.ShrinkMode | ShrinkMode.Reduction : a.ShrinkMode,
            ReportMode = a.ReportMode
        };
    }

    private readonly Func<T> model = model;
    private readonly Func<U> sut = sut;
    private readonly List<Func<bool, T, U, CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)>>> operations = operations;
    private readonly List<Observation<T, U>> observations = [observation];

    private CheckrOf<Case> GetCheckr()
    {
        var checkr =
            from m in Trackr.Stashed("Model", model)
            from s in Trackr.Stashed("Sut", sut)
            from showr in Showr.ForInput()
            from format in Combine.Checkrs(formatters)
            from ops in Checkr.OneOfWhen([.. operations.Select(a => a(verifyReturnValues, m, s))])
            from obs in Combine.Checkrs(observations.Select(a => a.Observe(m, s)))
            select Case.Closed;
        return checkr;
    }
}
