using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Bolts;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.CookBook.L_TheLoadedDie;

[DocFile]
[DocFileHeader("Testing Aggregate Behaviour (Trackr.Verify)")]
public class TheLoadedDie : BaseTest
{
    [CodeExample]
    public class DieSix
    {
        public int Roll() => new Random().Next(1, 6);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from die in Trackr.Stashed(() => new DieSix())
            from observer in Trackr.Stashed(() => new HashSet<int>())
            from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
            from verify in Trackr.Verify("Rolls",
                ("Contains 1", () => observer.Contains(1)),
                ("Contains 2", () => observer.Contains(2)),
                ("Contains 3", () => observer.Contains(3)),
                ("Contains 4", () => observer.Contains(4)),
                ("Contains 5", () => observer.Contains(5)),
                ("Contains 6", () => observer.Contains(6)))
            select Case.Closed;
    }

    [Fact]
    [DocContent(
@"**Demonstrates:**
- Accumulated-state testing
- `Trackr.Verify`
- `Trackr.StopWhenProven`

While not strictly speaking a property-test, as there is no shrinking involved,
this kind of test is nevertheless very useful.  
So much so that QuickCheckr provides a dedicated tool for it. 

We're testing a simple implementation of a die:
")]
    [DocExample(typeof(DieSix))]
    [DocContent("And we want to ensure that it rolls the numbers 1-6.  For which use the following Checkr:")]
    [DocExample(typeof(TheLoadedDie), nameof(GetCheckr))]
    [DocContent(
@"`Trackr.Verify` is the tool you reach for when the correctness of a test depends on the accumulated state at the end of a run,
rather than the outcome of any single execution.
In this Loaded Die example, each roll is recorded in a stashed HashSet<int>, and the *verify* is evaluated only after the final execution has completed.
This makes it ideal for tests where you want to confirm that a whole series of operations, as a group, produced the expected overall behaviour.

**Report:**")]
    [DocCodeFile("TheLoadedDie.txt", "text")]
    [DocContent(
@"Well, you probably saw this coming, but the die is bugged. `Random.Next` is upper-bound exclusive, so six is never rolled.  
A common mistake, and the verifier pointed it out in above report.

*Note:* `Trackr.Verify`never triggers shrinking, because there is no meaningful *minimal failing case*,
either the final state satisfies all verifications or it doesn't.")]
    public void RunTest()
    {
        var article = Journalist.Investigate(GetCheckr(), a => a.Run(1273117969, 100.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport<TheLoadedDie>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Rolls: Contains 6", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(100, article.Execution(1).Times);
        Assert.Equal("Roll", article.Execution(1).Action(1).Read().Label);
    }

    [CodeExample]
    public class DieSixFixed
    {
        public int Roll() => new Random().Next(1, 7);
    }

    [CodeRemove("from cnt in Checkr.Perform(() => ++cell.Value)")]
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr_Fixed(Cell<int> cell)
    {
        return
            from die in Trackr.Stashed(() => new DieSixFixed())
            from cnt in Checkr.Perform(() => ++cell.Value)
            from observer in Trackr.Stashed(() => new HashSet<int>())
            from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
            from proven in Trackr.StopWhenProven("All sides rolled",
                () => observer.Order().SequenceEqual([1, 2, 3, 4, 5, 6])) // <=
            from verify in Trackr.Verify("Rolls",
                ("Contains 1", () => observer.Contains(1)),
                ("Contains 2", () => observer.Contains(2)),
                ("Contains 3", () => observer.Contains(3)),
                ("Contains 4", () => observer.Contains(4)),
                ("Contains 5", () => observer.Contains(5)),
                ("Contains 6", () => observer.Contains(6)))
            select Case.Closed;
    }

    [Fact]
    [DocHeader("Stop! In the Name of ... Perf?")]
    [DocExample(typeof(DieSixFixed))]
    [DocContent(
@"Ok, we fixed the bug, so now our Checkr passes.
But what if we get lucky and we satisfy the verify after the first six rolls ?
Bit of a waste to continue running the Checkr, no ?  

Let's use `Trackr.StopWhenProven`:")]
    [DocExample(typeof(TheLoadedDie), nameof(GetCheckr_Fixed))]
    [DocContent(
@"Whenever the predicate passed into `Trackr.StopWhenProven` returns true,
the Checkr is considered proven and it concludes immediately, ... no more executions.")]
    public void RunTest_Fixed()
    {
        var counter = new Cell<int>(0);
        GetCheckr_Fixed(counter).Run(100.ExecutionsPerRun());
        Assert.True(counter.Value < 100);
    }
}