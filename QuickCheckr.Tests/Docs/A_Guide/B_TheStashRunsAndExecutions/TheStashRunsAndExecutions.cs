using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions;

[DocFile]
[DocFileHeader("The Stash, Runs and Executions")]
[DocContent(
@"Told you I was going to get back to you on that one (executions).

In the previous chapter we upped the run number in order to *fully explore the input space*.  
But what if it's not the input that causes the issue, but a side-effect.  
Consider:")]
[DocExample(typeof(BugHouse))]
public class TheStashRunsAndExecutions : BaseTest
{
    [CodeExample]
    public class BugHouse
    {
        private int count;
        public bool DelayedDetonation(int a)
        {
            if (count++ == 2 && a == 1) ComputerSays.No("I'm sorry, Dave, I'm afraid I can't do that.");
            return true;
        }
    }


    [CodeExample]
    private static readonly CheckrOf<Case> FaultyBugHouseCheckr =
        from input in Checkr.Input("Int", Fuzzr.Int())
        from callBugHouse in Checkr.Act("BugHouse.DelayedDetonation", () => new BugHouse().DelayedDetonation(input))
        from checkResult in Checkr.Spec("Returns true", () => callBugHouse)
        select Case.Closed;

    [Fact]
    [DocContent("If we want to test it and write a Checkr like so:")]
    [DocExample(typeof(TheStashRunsAndExecutions), nameof(FaultyBugHouseCheckr))]
    [DocContent(
@"The test will always pass, even if we increase the amount of runs to infinity,
because the test looses the state of the `BugHouse` between every run.")]
    public void ThisAlwaysPasses()
    {
        FaultyBugHouseCheckr.Run(10.Runs());
    }

    [CodeExample]
    private static readonly CheckrOf<Case> BugHouseCheckr =
        from bugHouse in Trackr.Stashed(() => new BugHouse())
        from input in Checkr.Input("Int", Fuzzr.Int())
        from callBugHouse in Checkr.Act("BugHouse.DelayedDetonation", () => bugHouse.DelayedDetonation(input))
        from checkResult in Checkr.Spec("Returns true", () => callBugHouse)
        select Case.Closed;


    [CodeRemove(", a => a with { Seed = 1854571642 }")]
    [CodeSnippet]
    private static void RunBugHouseCheckr()
    {
        BugHouseCheckr.Run(100.Runs(), 10.ExecutionsPerRun(), a => a with { Seed = 1854571642 });
    }

    [Fact]
    [DocContent(
@"So here's what we do:
1. We stash the `BugHouse` using `Trackr.Stashed` and instead of creating a new one in the `Act` we use the stashed one.
2. We up the number of executions per run.  
   The contents of `Trackr.Stashed` is reset after every run, but not in between executions.
   This allows us to *capture* the state of the `BugHouse`.  
   
**The Updated Checkr:**")]
    [DocExample(typeof(TheStashRunsAndExecutions), nameof(BugHouseCheckr))]
    [DocContent("**Running It:**")]
    [DocExample(typeof(TheStashRunsAndExecutions), nameof(RunBugHouseCheckr))]
    [DocContent("**The Report:**")]
    [DocCodeFile("TheStashRunsAndExecutions.txt", "text")]
    [DocContent(
@"The shrunk case makes it immediately clear why this Checkr needed multiple executions.  

QuickCheckr shows that the action BugHouse.DelayedDetonation was executed twice without issue,
and only on the third execution, where the relevant input `Int = 1` appears, does the exception occur.  

Because the BugHouse instance is stashed, its internal count persisted across executions, allowing the failure to surface.  

The shrinking process confirms that all three executions are required:
removing either of the first two would prevent the internal state from reaching the dangerous value.  

Finally, the report closes by listing which specs passed during the original run,
highlighting that the behaviour was stable until exactly the offending execution.")]
    [DocHeader("Recap")]
    [DocRawFile("TheStashRunsAndExecutionsRecap.md")]
    public void CheckingTheBugHouse()
    {
        var article = TheJournalist.Exposes(RunBugHouseCheckr);
        WriteAsserts(article);
        WriteReport<TheStashRunsAndExecutions>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(2, article.Execution(1).Read().Times);
        Assert.Equal("BugHouse.DelayedDetonation", article.Execution(1).Action(1).Read().Label);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("BugHouse.DelayedDetonation", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Int", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Returns true", article.PassedSpec(1).Read().Label);
        Assert.Equal(2, article.PassedSpec(1).Read().TimesPassed);
    }
}