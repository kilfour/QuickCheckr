using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOut;
using QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;
using QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions;

[DocFile]
[DocLink(typeof(InPoolsWeTrust))]
[DocLink(typeof(Guide))]
[DocLink(typeof(CheckringThingsOut))]
[DocLink(typeof(ChoosingTheNextMove))]
[DocFileHeader("The Stash, Runs and Executions")]
[DocContent("[Top][Guide] | [Previous][CheckringThingsOut] | [Next][ChoosingTheNextMove]  \n")]
[DocContent(
@"In the previous chapter we upped the run number in order to *fully explore the input space*.  
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
        from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
        select Case.Closed;

    [Fact]
    [DocContent("If we want to test it and write a Checkr like so:")]
    [DocExample(typeof(TheStashRunsAndExecutions), nameof(FaultyBugHouseCheckr))]
    [DocContent(
@"The test will always pass, even if we increase the amount of runs to infinity,
because the test loses the state of the `BugHouse` between every run.")]
    public void ThisAlwaysPasses()
    {
        FaultyBugHouseCheckr.Run(10.Runs());
    }

    [CodeExample]
    private static readonly CheckrOf<Case> BugHouseCheckr =
        from bugHouse in Trackr.Stashed(() => new BugHouse())
        from input in Checkr.Input("Int", Fuzzr.Int())
        from callBugHouse in Checkr.Act("BugHouse.DelayedDetonation", () => bugHouse.DelayedDetonation(input))
        from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
        select Case.Closed;

    private static CheckrConfig Configure(CheckrConfig cfg)
        => cfg with { ReportMode = cfg.ReportMode & ~ReportMode.StackTrace };

    [CodeReplace("1854571642", "100.Runs()")]
    [CodeRemove(", Configure")]
    [CodeSnippet]
    private static void RunBugHouseCheckr()
    {
        BugHouseCheckr.Run(1854571642, 10.ExecutionsPerRun(), Configure);
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

Finally, the report closes by listing which expectations passed during the original run,
highlighting that the behaviour was stable until exactly the offending execution.")]
    [DocHeader("Recap")]
    [DocContent(
@"QuickCheckr operates across *two distinct scopes*, and understanding them is essential when testing stateful systems:

**1. Run Scope**

A *run* is a full pass through your Checkr script.
Every run starts with a clean slate:

* All `Trackr.Stashed` values are reset.
* Pools (explained [later][InPoolsWeTrust]) start empty.
* Counters, uniqueness constraints, and any internal memories tied to a run are cleared.

This guarantees that each run is **independent**, **repeatable**, and **reproducible** based on the seed.

**But inside a run**, QuickCheckr may execute your script multiple times.

### **2. Execution Scope**

An *execution* is one walk through the Checkr script *within* a single run.
Executions share:

* The **same stashed instances** (e.g. the `BugHouse`).
* The **same pools**, which accumulate objects over executions.
* The **effects** of all previous executions in the run.

Meanwhile, the following *are fresh each execution*:

* Every `Checkr.Input` value.
* Every `Checkr.Act`, `Checkr.Expect`, and `Checkr.Trace` is evaluated anew.
* The shrinking engine can re-run isolated executions to evaluate relevance.

This means:

* **Runs give you independence.**
* **Executions give you statefulness.**

And together, they let QuickCheckr explore both *pure input variation* and *multi-step behaviour*,
which is exactly what makes the BugHouse example catchable:
the failure only emerges when several executions act on the *same* object.
")]
    [DocContent("\n[Top][Guide] | [Previous][CheckringThingsOut] | [Next][ChoosingTheNextMove]")]
    // **!! Important !! :** The static factory classes follow the same scoping model:
    // - `Checkr.*` combinators are execution-scoped: they run once per execution and do not persist state.
    // - `Trackr.*` combinators are run-scoped: they hold state that persists across all executions within a run.
    public void CheckingTheBugHouse()
    {
        var article = TheJournalist.Exposes(RunBugHouseCheckr);
        WriteAsserts(article, false);
        WriteReport<TheStashRunsAndExecutions>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: I'm sorry, Dave, I'm afraid I can't do that.", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(2, article.Execution(1).Read().Times);
        Assert.Equal("BugHouse.DelayedDetonation", article.Execution(1).Action(1).Read().Label);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("BugHouse.DelayedDetonation", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Int", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Returns true", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }
}