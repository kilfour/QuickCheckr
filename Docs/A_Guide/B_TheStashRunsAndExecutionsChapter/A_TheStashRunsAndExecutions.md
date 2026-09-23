# The Stash, Runs and Executions
|[◀][A_CheckringThingsOut]|[△][Guide]|[▶][A_ChoosingTheNextMove]|
|---|---|---|
  
In the previous chapter we upped the number of runs in order to explore more of the input space.  
But what if it's not the input that causes the issue, but a side-effect ?

Consider the following model:  
```csharp
public class BugHouse
{
    private int count;
    public bool DelayedDetonation(int a)
    {
        if (count++ == 2 && a == 1)
            throw new Exception("I'm sorry, Dave, I'm afraid I can't do that.");
        return true;
    }
}
```
If we want to test it and write a Checkr like so:  

**The Checkr:**  
```csharp
from input in Checkr.Input("Int", Fuzzr.Int())
from callBugHouse in Checkr.Act("DelayedDetonation", () => new BugHouse().DelayedDetonation(input))
from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
select Case.Closed;
```
The test will always pass, even if we increase the amount of runs indefinitely,
because the test loses the state of the `BugHouse` between every run.  
So here's what we do:
1. We stash the `BugHouse` using `Trackr.Stashed` and instead of creating a new one in the `Act` we use the stashed one.
2. We up the number of executions per run.  
   Values stored with `Trackr.Stashed` are reset after every run, but not in between executions.
   This allows us to *capture* the state of the `BugHouse`.  

**The Checkr:**  
```csharp
from bugHouse in Trackr.Stashed(() => new BugHouse())
from input in Checkr.Input("Int", Fuzzr.Int())
from callBugHouse in Checkr.Act("DelayedDetonation", () => bugHouse.DelayedDetonation(input))
from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
select Case.Closed;
```

**Running It:**  
```csharp
checkr.Run(100.Runs(), 10.ExecutionsPerRun());
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_DelayedDetonationFound.cs:93:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1854571642
 ------------------------------------------------------------
  Executed: DelayedDetonation (2 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: DelayedDetonation
   - Int = 1
 ============================================================
  !! Exception: I'm sorry, Dave, I'm afraid I can't do that.
 ============================================================
 Passed Expectations
 - Returns true: 2x
 ------------------------------------------------------------
```
The shrunk case makes it immediately clear why this Checkr needed multiple executions.  

QuickCheckr shows that the action `DelayedDetonation` was executed twice without issue,
and only on the third execution, where the relevant input `Int = 1` appears, does the exception occur.  

Because the `BugHouse` instance is stashed, its internal `count` persisted across executions, allowing the failure to surface.  

The shrinking process confirms that all three executions are required:
removing either of the first two would prevent the internal state from reaching the dangerous value.  

Finally, the report closes by listing which expectations passed during the original run,
highlighting that the behaviour was stable until exactly the offending execution.  
### Recap
QuickCheckr operates across *two distinct scopes*, and understanding them is essential when testing stateful systems:

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
* Every `Checkr.Act` and `Checkr.Expect` is evaluated anew.
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
which is exactly what makes the `BugHouse` example catchable:
the failure only emerges when several executions act on the *same* object.
  

[InPoolsWeTrust]: ../D_InPoolsWeTrust/InPoolsWeTrust.md

[Guide]: ../Guide.md

[A_CheckringThingsOut]: ../A_CheckringThingsOutChapter/A_CheckringThingsOut.md

[A_ChoosingTheNextMove]: ../C_ChoosingTheNextMoveChapter/A_ChoosingTheNextMove.md
