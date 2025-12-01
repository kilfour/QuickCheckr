using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.E_Examples.L_TheLoadedDie;

[DocFile]
public class TheLoadedDie : BaseTest
{
    [CodeExample]
    public class DieSix
    {
        public int Roll() => new Random().Next(1, 6);
    }

    [Fact]
    [DocContent(
@"While not strictly speaking a property-test, as there is no shrinking involved,
this kind of test is nevertheless very useful.  
So much so that QuickCheckr so much so that QuickCheckr provides a dedicated tool for it. 

We're testing a simple implementation of a die:
")]
    [DocExample(typeof(DieSix))]
    [DocContent("And we want to ensure that it rolls the numbers 1-6.  For which use the following Checkr:")]
    [DocExample(typeof(TheLoadedDie), nameof(GetCheckr))]
    [DocContent(
@"`Trackr.Assay` is the tool you reach for when the correctness of a test depends on the accumulated state at the end of a run,
rather than the outcome of any single execution.
In this Loaded Die example, each roll is recorded in a stashed HashSet<int>, and the *assay* is evaluated only after the final execution has completed.
This makes it ideal for tests where you want to confirm that a whole series of operations, as a group, produced the expected overall behaviour.

**Report:**")]
    [DocCodeFile("TheLoadedDie.txt", "text")]
    [DocContent(
@"Well, you probably saw this coming, but the die is bugged. `Random.Next` is upper-bound exclusive, so six is never rolled.  
A common mistake, and the assayer pointed it out in above report.

*Sidenote:* When one uses `Trackr.Assay` there is no shrinking whatsoever, as there is no *minimal* fail case, by definition.  
Assays evaluate state accumulated across all executions, so removing any execution and or inputs,
changes the final state therefore shrinking is not applicable.")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.ExecutionsPerRun(), a => a with { Seed = 930735070 }));
        WriteAsserts(article);
        WriteReport<TheLoadedDie>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Rolls: Contains 6", article.AssayerDisagrees());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(100, article.Execution(1).Read().Times);
        Assert.Equal("Roll", article.Execution(1).Action(1).Read().Label);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from die in Trackr.Stashed(() => new DieSix())
            from observer in Trackr.Stashed(() => new HashSet<int>())
            from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
            from assay in Trackr.Assay("Rolls",
                ("Contains 1", () => observer.Contains(1)),
                ("Contains 2", () => observer.Contains(2)),
                ("Contains 3", () => observer.Contains(3)),
                ("Contains 4", () => observer.Contains(4)),
                ("Contains 5", () => observer.Contains(5)),
                ("Contains 6", () => observer.Contains(6)))
            select Case.Closed;
    }
}