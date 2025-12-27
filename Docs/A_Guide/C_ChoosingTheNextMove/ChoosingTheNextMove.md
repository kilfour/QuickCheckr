# Choosing the Next Move
[Top][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  
  
In the previous chapter we mentioned *Pools* and promised to explain how they work.  
We'll get there soon, but first we need to talk about `Checkr.OneOf`, the tool that lets QuickCheckr explore branching behaviour.

`OneOf` is QuickCheckr's way of saying:

> **"At this step, choose *exactly one* of these possible behaviours and execute it."**

It is the foundational building block for introducing randomness into *what happens next*.
Instead of describing one action, you describe a *set* of alternative next moves,
and QuickCheckr picks one at random during each execution.

Let's keep the model simple:  
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```
Suppose our system has a (not implemented here) 'no overdraft' invariant.
The bug is trivial, but that's fine: we're here to demonstrate *behavioural branching*, not banking security.  
## A Checkr With a Choice
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int())
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
Every execution through this Checkr does the same things:

1. Generate a random amount.
2. Pick exactly **one** action: `Deposit` *or* `Withdraw`.
3. Check the invariant.

A key detail:

> **Each branch runs inside the same execution scope.**
> This means both branches mutate the same stashed `account` instance.

So across executions, QuickCheckr builds a sequence of deposits and withdrawals, any of which may lead to a failure.
  
## Seeing Which Branches Were Chosen
Running the Checkr without shrinking yields a report similar to this:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:65:1
 Original failing run:    5 executions
 Seed:                    1509195528
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 61
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 13
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 61
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 20
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 98
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 4x
 ------------------------------------------------------------
```
This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.  
## Shrinking Branching Behaviour
And if we let the QuickCheckr shrinker do its work:  
```text
------------------------------------------------------------
 Test:                    RunTest_Shrunk
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:133:1
 Original failing run:    5 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1509195528
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 98
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 4x
 ------------------------------------------------------------
```
The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
  
## On One Condition
In many cases you would like to control the possible branches taken, based on some condition.  
For this you can use `Checkr.OneOfWhen`, which takes an enumerable of `Checkr.Option` elements.  

**Example:**  
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
**Results in:**  
```text
------------------------------------------------------------
 Test:                    RunTest_Conditional
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:183:1
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
## Recap
`Checkr.OneOf` and `Checkr.OneOfWhen` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.  

[Top][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  

[Guide]: ../Guide.md

[TheStashRunsAndExecutions]: ../B_TheStashRunsAndExecutions/TheStashRunsAndExecutions.md

[InPoolsWeTrust]: ../D_InPoolsWeTrust/InPoolsWeTrust.md
