# Choosing The Next Move
In the previous chapter we mentioned *Pools* and promised to explain how they work, but before that we have to look at `Checkr.OneOf`

Here's the model we're using:  
```csharp
public class Account
{
    public int Balance { get; private set; } = 0;
    public void Deposit(int amount)
    {
        Balance += amount;
        ApplyFee();
    }
    public void WithDraw(int amount)
    {
        if (amount > Balance) return;
        Balance -= amount;
        ApplyFee();
    }
    private void ApplyFee() => Balance -= 1;
}
```
As you can see there's a (badly implemented) *no overdraft* invariant in the system.  
Let's find out whether or not we can find the bug.  
So here's the Checkr:  
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int())
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
Report:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:47:1
 Original failing run:    3 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    1254202696
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 24
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 23
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 2x
 ------------------------------------------------------------
```
