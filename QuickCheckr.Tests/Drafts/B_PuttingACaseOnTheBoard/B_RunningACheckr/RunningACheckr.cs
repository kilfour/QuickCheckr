using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard;
using QuickCheckr.Tests.Tools;
using QuickPulse.Bolts;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.B_RunningACheckr;

[DocFile]
[DocFileHeader("Running a Checkr")]
[DocLink(typeof(Configuration))]
[DocLink(typeof(PuttingACaseOnTheBoard))]
[DocContent("[The Board][PuttingACaseOnTheBoard] | [Next][Configuration]  \n")]
[DocContent(
@"This is done by calling `Run` on the Checkr instance.  
Several overloads are available:")]
public class RunningACheckr : BaseTest
{
    private static CheckrOf<Case> GetCheckr(Cell<int> runCount, Cell<int> executionCount) =>
        from cntRuns in Trackr.Stashed(() => { runCount.Value++; return 0; })
        from clearExecutions in Trackr.Stashed(() => { executionCount.Value = 0; return "Execution"; })
        from cntExecutions in Checkr.Perform(() => executionCount.Value++)
        from _ in Checkr.Expect("Always true", () => true)
        select Case.Closed;

    [CodeSnippet]
    private static void Do_Minimal(CheckrOf<Case> checkr)
        => checkr.Run();

    [Fact]
    [DocContent("- Run the Checkr once with a single execution.")]
    [DocExample(typeof(RunningACheckr), nameof(Do_Minimal))]
    public void Minimal()
    {
        var runCount = new Cell<int>(0);
        var executionCount = new Cell<int>(0);
        var checkr = GetCheckr(runCount, executionCount);
        Do_Minimal(checkr);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(1, executionCount.Value);
    }
    [CodeSnippet]
    private static void Do_RunsOnly(CheckrOf<Case> checkr)
        => checkr.Run(5.Runs());

    [Fact]
    [DocContent("- Run the Checkr multiple times, each consisting of a single execution.")]
    [DocExample(typeof(RunningACheckr), nameof(Do_RunsOnly))]
    public void RunsOnly()
    {
        var runCount = new Cell<int>(0);
        var executionCount = new Cell<int>(0);
        var checkr = GetCheckr(runCount, executionCount);
        Do_RunsOnly(checkr);
        Assert.Equal(5, runCount.Value);
        Assert.Equal(1, executionCount.Value);
    }

    [CodeSnippet]
    private static void Do_ExecutionsOnly(CheckrOf<Case> checkr)
        => checkr.Run(5.ExecutionsPerRun());

    [Fact]
    [DocContent("- Run the Checkr once but performs multiple executions within that run.")]
    [DocExample(typeof(RunningACheckr), nameof(Do_ExecutionsOnly))]
    public void ExecutionsOnly()
    {
        var runCount = new Cell<int>(0);
        var executionCount = new Cell<int>(0);
        var checkr = GetCheckr(runCount, executionCount);
        Do_ExecutionsOnly(checkr);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(5, executionCount.Value);
    }

    [CodeSnippet]
    private static void Do_Full(CheckrOf<Case> checkr)
        => checkr.Run(3.Runs(), 5.ExecutionsPerRun());

    [Fact]
    [DocContent("- Run the Checkr with the specified number of runs and executions per run.")]
    [DocExample(typeof(RunningACheckr), nameof(Do_Full))]
    public void Full()
    {
        var runCount = new Cell<int>(0);
        var executionCount = new Cell<int>(0);
        var checkr = GetCheckr(runCount, executionCount);
        Do_Full(checkr);
        Assert.Equal(3, runCount.Value);
        Assert.Equal(5, executionCount.Value);
    }

    [CodeSnippet]
    private static void Do_SeedOnly(CheckrOf<Case> checkr)
        => checkr.Run(424242);

    [Fact]
    [DocContent("- Run the Checkr with the specified seed and one execution.")]
    [DocExample(typeof(RunningACheckr), nameof(Do_SeedOnly))]
    public void SeedOnly()
    {
        var runCount = new Cell<int>(0);
        var executionCount = new Cell<int>(0);
        var checkr = GetCheckr(runCount, executionCount);
        Do_SeedOnly(checkr);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(1, executionCount.Value);
    }

    [Fact]
    public void SeedOnlySet()
    {
        var article = TheJournalist.Exposes(
            () => Checkr.Expect("Fails", () => false).Run(424242));
        Assert.Equal(424242, article.Seed());
    }

    [CodeSnippet]
    private static void Do_Seed(CheckrOf<Case> checkr)
        => checkr.Run(424242, 5.ExecutionsPerRun());

    [Fact]
    [DocContent("- Run the Checkr with the specified seed and the specified number of executions.")]
    [DocExample(typeof(RunningACheckr), nameof(Do_Seed))]
    public void Seed()
    {
        var runCount = new Cell<int>(0);
        var executionCount = new Cell<int>(0);
        var checkr = GetCheckr(runCount, executionCount);
        Do_Seed(checkr);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(5, executionCount.Value);
    }

    [Fact]
    public void SeedSet()
    {
        var article = TheJournalist.Exposes(
            () => Checkr.Expect("Fails", () => false).Run(424242, 5.ExecutionsPerRun()));
        Assert.Equal(424242, article.Seed());
    }

    [Fact]
    [DocContent(
@"All of the methods mentioned above can take an optional parameter `Func<CheckrConfig, CheckrConfig> configure`
which allows for controlling specific aspects of the Checkr run.")]
    [DocContent("\n[The Board][PuttingACaseOnTheBoard] | [Previous][TestFrameworkIntegration] | [Next][Configuration]")]
    public void WithConfigurations()
    {

        var runCount = new Cell<int>(0);
        var executionCount = new Cell<int>(0);
        var checkr = GetCheckr(runCount, executionCount);

        checkr.Run(a => a);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(1, executionCount.Value);

        runCount.Value = 0;
        executionCount.Value = 0;
        checkr.Run(5.Runs(), a => a);
        Assert.Equal(5, runCount.Value);
        Assert.Equal(1, executionCount.Value);

        runCount.Value = 0;
        executionCount.Value = 0;
        checkr.Run(5.ExecutionsPerRun(), a => a);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(5, executionCount.Value);

        runCount.Value = 0;
        executionCount.Value = 0;
        checkr.Run(3.Runs(), 5.ExecutionsPerRun(), a => a);
        Assert.Equal(3, runCount.Value);
        Assert.Equal(5, executionCount.Value);

        runCount.Value = 0;
        executionCount.Value = 0;
        checkr.Run(424242, a => a);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(1, executionCount.Value);
        var article = TheJournalist.Exposes(
            () => Checkr.Expect("Fails", () => false).Run(424242, a => a));
        Assert.Equal(424242, article.Seed());

        runCount.Value = 0;
        executionCount.Value = 0;
        checkr.Run(424242, 5.ExecutionsPerRun(), a => a);
        Assert.Equal(1, runCount.Value);
        Assert.Equal(5, executionCount.Value);
        article = TheJournalist.Exposes(
            () => Checkr.Expect("Fails", () => false).Run(424242, 5.ExecutionsPerRun(), a => a));
        Assert.Equal(424242, article.Seed());
    }
}