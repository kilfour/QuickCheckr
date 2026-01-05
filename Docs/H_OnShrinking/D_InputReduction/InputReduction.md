# Input Reduction
After InputShrinking has classified all inputs as either relevant or irrelevant, any registered *reducers*
are run on the remaining relevant inputs (example below).  

Note the use of the word *registered*. Staying true to it's shrinking philosophy,
QuickChecker does not try and guess how to reduce input values. It's up to the user to supply that information.  

Most of the time, even in the example here I think, reducers are not needed to figure out what went wrong from the report.  
But in some cases it is a must.  

So what is a reducer ?  

Well let me show you.  
Using the `Account` example from the guide:  

**The Model:**  
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```

**The Checkr:**  
We're running the same checkr, with the same seed.
The only difference is the addition of `Reducr.TowardsZero` in the `Checkr.Input` argument list.  
```csharp
from account in Trackr.Stashed(() => new Account())
from amount in Checkr.Input("Amount", Fuzzr.Int(), Reduce.TowardsZero) // <=
from operations in Checkr.OneOf(
    Checkr.Act("Deposit", () => account.Deposit(amount)),
    Checkr.Act("Withdraw", () => account.WithDraw(amount)))
from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                InputReduction.cs:57:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1684147193
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 87
            Redux    1
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 1x
 ------------------------------------------------------------
```
And as you can see it does exactly what it says on the tin. 
It *reduces* the original value using a user supplied algorithm.  
This one just decrements the input until (and including) it reaches zero, and checks if the run still fails.
If the run passes, the value last tried is reported as the *reduced* input.  

In the example here, `0` was tried, but it made the test pass, so the previously tried candidate `1` is reported.  

QuickCheckr comes with a small number of built-in reducers, but it is relatively easy to implement your own.  
Depending on demand, the library of reducers might grow fast.

TODO: LINK TO REDUCE REUSE RECYCLE
  
