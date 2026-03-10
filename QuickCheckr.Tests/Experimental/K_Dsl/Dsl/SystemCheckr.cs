using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Experimental.K_Dsl.Dsl;


public class SystemCheckr
{
    public static SystemCheckr Named(string name) => new(name);

    private Func<CheckrConfig, CheckrConfig> configure;

    private SystemCheckr(
        string name
    )
    {
        configure = cfg => cfg with { FileAs = name };
    }

    private Operation[] operations = [];
    public SystemCheckr Operations(params Operation[] operations) { this.operations = operations; return this; }

    private readonly List<CheckrOf<Case>> pools = [];
    public SystemCheckr PoolOf<T>()
    {
        pools.Add(Trackr.PoolFor<T>());
        return this;
    }

    private readonly List<CheckrOf<Case>> invariants = [];

    public SystemCheckr ForAll<T>(string label, Func<T, bool> expectation)
    {
        invariants.Add(Trackr.PoolExpectEach<T>(label, a => expectation(a)));
        return this;
    }

    public SystemCheckr DisableWarnings()
    {
        var previous = configure;
        configure = cfg =>
        {
            var configured = previous(cfg);
            return configured with { ReportMode = configured.ReportMode & ~ReportMode.Warning };
        };
        return this;
    }

    public void Run(CheckrOfTRun.RunCount runs, CheckrOfTRun.ExecutionCount executionsPerRun) =>
        TheCheckr().Run(runs, executionsPerRun, configure);

    public void Run(int seed, CheckrOfTRun.ExecutionCount executionsPerRun) =>
        TheCheckr().Run(seed, executionsPerRun, configure);

    private CheckrOf<Case> TheCheckr() =>
        from pools in Combine.Checkrs(pools)
        from api in Trackr.Stashed(() => new SystemUnderTest())
        from _ in api.Operations(operations)
        from invariants in Combine.Checkrs(invariants)
        select Case.Closed;
}
