# Choosing The Next Move
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
 Original failing run:    8 executions
 Seed:                    1169776765
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 71
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 5
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 31
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 15
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 56
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 94
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 36
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 66
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 7x
 ------------------------------------------------------------
```
This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.  
## Shrinking Branching Behaviour
And if we let the QuickCheckr shrinker do its work:  
```text
------------------------------------------------------------
 Test:                    RunTest_Shrunk
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:145:1
 Original failing run:    8 executions
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    1169776765
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 66
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 7x
 ------------------------------------------------------------
```
The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
* The amount was positive, so withdrawing it immediately violates the invariant.

This highlights an important point:

> **QuickCheckr does not explore alternative branches during shrinking.**  
> It only shrinks *what actually happened*.

Shrinking removes irrelevant **executions**, trims **inputs**, and discards everything that had no influence on the failure.
The branch chosen during the original failing run is *the* branch examined during shrinking.   
## Recap
`Checkr.OneOf` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.
* Shrinking works naturally: QuickCheckr does **not** try unchosen branches. 
  It only minimizes the actual failing scenario.  
