# Choosing the Next Move
|[◀][A_TheStashRunsAndExecutions]|[△][Guide]|[▶][InPoolsWeTrust]|
|---|---|---|  
In the previous chapter we mentioned *Pools* and promised to explain how they work.  
We'll get there soon, but first we need to talk about `Checkr.OneOf`, the tool that lets QuickCheckr explore branching behaviour.

`OneOf` is QuickCheckr's way of saying:

> **"At this step, choose *exactly one* of these possible behaviours and execute it."**

It is the foundational building block for introducing randomness into *what happens next*.
Instead of describing one action, you describe a *set* of alternative next moves,
and QuickCheckr picks one at random during each execution.

`OneOf` turns your Checkr into a **state machine explorer**.

Let's keep the model simple:  
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount)
    {
        if (amount == 42)
            ComputerSays.No("No answers for you.");
        Balance -= amount;
    }
}
```
## A Checkr With a Choice
```csharp
from account in Trackr.Stashed(() => new Account())
from amount in Checkr.Input("Amount", Fuzzr.Int())
from operations in Checkr.OneOf(
    Checkr.Act("Deposit", () => account.Deposit(amount)),
    Checkr.Act("Withdraw", () => account.WithDraw(amount))
)
select Case.Closed;
```
Every execution through this Checkr does the same things:

1. Generate a random amount.
2. Pick exactly **one** action: `Deposit` *or* `Withdraw`.

A key detail:

> **Each branch runs inside the same *Run Scope*.**
> This means both branches mutate the same stashed `account` instance.

So across executions, QuickCheckr builds a sequence of deposits and withdrawals, any of which may lead to a failure.

**Note:**  
> Any exception thrown during execution of a `Checkr.Act` operation will be caught and considered a failure.
  
## Seeing Which Branches Were Chosen
Running the Checkr without shrinking yields a report similar to this:  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ChoosingTheNextMove.cs:90:1
 Original failing run:    5 executions
 Seed:                    971854269
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 55
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 30
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 79
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 21
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 42
 ===========================================================================
  !! ComputerSaysNo: No answers for you.
 ===========================================================================
```
This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.  
## Shrinking Branching Behaviour
And if we let the QuickCheckr shrinker do its work:  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ChoosingTheNextMoveShrunk.cs:73:1
 Original failing run:    5 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    971854269
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 42
 ===========================================================================
  !! ComputerSaysNo: No answers for you.
 ===========================================================================
```
The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
  
## On One Condition
In many cases you would like to control the possible branches taken, based on some condition.  
For this you can use `Checkr.OneOfWhen`, which takes an enumerable of `Checkr.Option` elements.  

**Example:**  

**The Checkr:**  
```csharp
from everything in Trackr.Stashed(() => new Everything())
from operations in Checkr.OneOfWhen(
    Checkr.Option(() => everything.TheUniverse == null,
        from genesis in Checkr.Act("Create the universe",
            () => everything.TheUniverse = new Universe())
        select Case.Closed),
    Checkr.Option(() => everything.TheUniverse != null,
        from falsify in Checkr.Act("Falsify the answer",
            () => everything.TheUniverse!.Answer++)
        select Case.Closed)
)
from checkTheAnswer in Checkr.ExpectWhen("is 42",
    () => everything.TheUniverse != null,
    () => everything.TheUniverse!.Answer == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ChoosingTheNextMoveConditional.cs:73:1
 Original failing run:    2 executions
 Seed:                    356083796
 ------------------------------------------------------------
  Executed: Create the universe
 ------------------------------------------------------------
  Executed: Falsify the answer
 ================================
  !! Expectation Failed: is 42
 ================================
 Passed Expectations
 - is 42: 1x
 ------------------------------------------------------------
```
### Recap
`Checkr.OneOf` and `Checkr.OneOfWhen` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.  

[Guide][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  

[Guide]: ../Guide.md

[A_TheStashRunsAndExecutions]: ../B_TheStashRunsAndExecutionsChapter/A_TheStashRunsAndExecutions.md

[InPoolsWeTrust]: ../D_InPoolsWeTrust/InPoolsWeTrust.md
