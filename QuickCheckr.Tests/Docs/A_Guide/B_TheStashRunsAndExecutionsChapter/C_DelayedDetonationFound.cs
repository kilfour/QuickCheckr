using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutionsChapter;


public class C_DelayedDetonationFound : CheckrTest<C_DelayedDetonationFound>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent(
@$"So here's what we do:
1. We stash the `{nameof(A_TheStashRunsAndExecutions.BugHouse)}` using `{nameof(Trackr)}.{nameof(Trackr.Stashed)}` and instead of creating a new one in the `Act` we use the stashed one.
2. We up the number of executions per run.  
   Values stored with `{nameof(Trackr)}.{nameof(Trackr.Stashed)}` are reset after every run, but not in between executions.
   This allows us to *capture* the state of the `{nameof(A_TheStashRunsAndExecutions.BugHouse)}`.")]
    [DocCheckr]
    [DocBoldHeader("Running It")]
    [DocExample(typeof(C_DelayedDetonationFound), nameof(RunCheckr))]
    [DocReport]
    [DocContent(
@$"The shrunk case makes it immediately clear why this Checkr needed multiple executions.  

QuickCheckr shows that the action `{nameof(A_TheStashRunsAndExecutions.BugHouse.DelayedDetonation)}` was executed twice without issue,
and only on the third execution, where the relevant input `Int = 1` appears, does the exception occur.  

Because the `{nameof(A_TheStashRunsAndExecutions.BugHouse)}` instance is stashed, its internal `count` persisted across executions, allowing the failure to surface.  

The shrinking process confirms that all three executions are required:
removing either of the first two would prevent the internal state from reaching the dangerous value.  

Finally, the report closes by listing which expectations passed during the original run,
highlighting that the behaviour was stable until exactly the offending execution.")]
    [DocHeader("Recap")]
    [DocContent(
@$"QuickCheckr operates across *two distinct scopes*, and understanding them is essential when testing stateful systems:

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

* The **same stashed instances** (e.g. the `{nameof(A_TheStashRunsAndExecutions.BugHouse)}`).
* The **same pools**, which accumulate objects over executions.
* The **effects** of all previous executions in the run.

Meanwhile, the following *are fresh each execution*:

* Every `Checkr.Input` value.
* Every `Checkr.Act`, `Checkr.Expect`, and `Checkr.Trace` is evaluated anew.
* The shrinking engine can re-run isolated executions to evaluate relevance.

```text
RUN 1: [Execution 1] => [Execution 2] => [Execution 3] (fails)  
       ↑_________________same stash__________________↑
       
RUN 2: Fresh stash, fresh start.
```

This means:

* **Runs give you independence.**
* **Executions give you statefulness.**

And together, they let QuickCheckr explore both *pure input variation* and *multi-step behaviour*,
which is exactly what makes the `{nameof(A_TheStashRunsAndExecutions.BugHouse)}` example catchable:
the failure only emerges when several executions act on the *same* object.
")]
    public override void Example() => Document(RunCheckr);

    [CodeReplace("1854571642", "100.Runs()")]
    [CodeSnippet]
    private static void RunCheckr(ConfiguredCheckr checkr)
    {
        checkr.Run(1854571642, 10.ExecutionsPerRun());
    }

    [CodeSnippet]
    [CodeRemove("A_TheStashRunsAndExecutions.")]
    protected override CheckrOf<Case> TheCheckr() =>
        from bugHouse in Trackr.Stashed(() => new A_TheStashRunsAndExecutions.BugHouse())
        from input in Checkr.Input("Int", Fuzzr.Int())
        from callBugHouse in Checkr.Act("DelayedDetonation", () => bugHouse.DelayedDetonation(input))
        from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Exception: I'm sorry, Dave, I'm afraid I can't do that.", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(2, article.Execution(1).Times);
        Assert.Equal("DelayedDetonation", article.Execution(1).Action(1).Read().Label);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("DelayedDetonation", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Int", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Returns true", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }
}