using System.Diagnostics;
using QuickCheckr;
using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.Protocol.Custodians;


namespace QuickTestr.Bolts.Builders;

/// <summary>
/// Provides the shared execution and vault behavior for concrete Testr runners.
/// Use as the base implementation for property-based and oracle-based runner types.
/// </summary>
public abstract class TestrRunner<TInput>
{
    internal string fileName = string.Empty;
    internal virtual string TestName { get; } = string.Empty;
    internal ICustodian? custodian;

    internal TestrRunner() { }

    /// <summary>Awaits the Testr using the default 100 runs. Use for callbacks that may suspend.</summary>
    [StackTraceHidden]
    public Task<TestrRunner<TInput>> RunAsync(CancellationToken cancellationToken = default)
        => RunAsync(100.Runs(), cancellationToken);

    /// <summary>Awaits a reproducible run using the specified seed.</summary>
    [StackTraceHidden]
    public async Task<TestrRunner<TInput>> RunAsync(int seed, CancellationToken cancellationToken = default)
    {
        await GetCheckr().Configure(GetConfig()).RunAsync(seed, cancellationToken).ConfigureAwait(false);
        return this;
    }

    /// <summary>Awaits the specified number of runs, including shrinking and reporting.</summary>
    [StackTraceHidden]
    public async Task<TestrRunner<TInput>> RunAsync(RunCount tries, CancellationToken cancellationToken = default)
    {
        await GetCheckr().Configure(GetConfig()).RunAsync(tries, cancellationToken).ConfigureAwait(false);
        return this;
    }


    /// <summary>
    /// Runs the Testr using the default number of runs.
    /// Use for the normal execution path when you do not need explicit configuration.
    /// </summary>
    [StackTraceHidden]
    public TestrRunner<TInput> Run()
        => Run(100.Runs());

    /// <summary>
    /// Runs the Testr using the specified seed.
    /// Use when you want a reproducible execution of a known case.
    /// </summary>
    [StackTraceHidden]
    public TestrRunner<TInput> Run(int seed)
    {
        GetCheckr().Configure(GetConfig()).Run(seed);
        return this;
    }


    /// <summary>
    /// Runs the Testr using the specified number of runs.
    /// Use when you want to control how much search effort is spent.
    /// </summary>
    [StackTraceHidden]
    public TestrRunner<TInput> Run(RunCount tries)
    {
        GetCheckr().Configure(GetConfig()).Run(tries);
        return this;
    }

    /// <summary>
    /// Persists case files for this Testr under its test name.
    /// Use when you want to inspect or clean up stored cases later through the vault workflow.
    /// </summary>
    public TestrRunner<TInput> StoreCaseFiles(ICustodian? custodian = null)
    {
        fileName = TestName;
        this.custodian = custodian;
        return this;
    }

    /// <summary>
    /// Searches for distinct failing cases and stores them in the vault.
    /// Use when you want a representative set of different failing inputs for the same Testr.
    /// Inputs are grouped by value and a limited set of failures (10) is retained.
    /// </summary>
    public TestrRunner<TInput> FillVault(
        SearchCount searchCount,
        RunCount runs) =>
            FillVault(searchCount, runs, VaultPolicy<TInput>.Default);

    /// <summary>
    /// Searches for distinct failing cases and stores them in the vault using a custom policy.
    /// Use when you want to control how failures are grouped, limited, or skipped during vault filling.
    /// </summary>
    public TestrRunner<TInput> FillVault(
        SearchCount searchCount,
        RunCount runs,
        VaultPolicy<TInput> policy)
    {
        GetCheckr(policy.ClassifyBy, policy.SkipWhen)
            .Configure(AddFileAsToConfig())
            .Conduct(
                searchCount.NumberOfSearches.Investigations(),
                runs,
                1.ExecutionsPerRun(),
                new Directive
                {
                    MaxCaseFiles = policy.MaxStoredFailures
                });
        return this;
    }


    /// <summary>
    /// Re-runs the stored vault cases and reports which ones still fail.
    /// Use to review persisted seeds after code changes or fixes.
    /// </summary>
    public TestrRunner<TInput> InspectVault()
    {
        GetCheckr()
            .Configure(AddFileAsToConfig())
            .ReviewColdCases();
        return this;
    }


    /// <summary>
    /// Removes or closes vault cases that no longer reproduce.
    /// Use to keep the vault focused on still-relevant failures.
    /// </summary>
    public TestrRunner<TInput> CleanupVault()
    {
        GetCheckr()
            .Configure(AddFileAsToConfig())
            .CloseResolvedColdCases();
        return this;
    }

    public TestrRunner<TInput> Debug(int seed, AutopsyProbe autopsyProbe)
    {
        GetCheckr().Configure(GetConfig()).Autopsy(seed, 1.ExecutionsPerRun(), autopsyProbe);
        return this;
    }


    private Func<CheckrConfig, CheckrConfig> AddFileAsToConfig()
        => a => GetConfig()(a) with { FileAs = TestName };

    internal abstract CheckrOf<Case> GetCheckr(
        Func<TInput, object>? distinguish = null,
        Func<TInput, bool>? rejectCaseWhen = null);
    internal abstract Func<CheckrConfig, CheckrConfig> GetConfig();
}
