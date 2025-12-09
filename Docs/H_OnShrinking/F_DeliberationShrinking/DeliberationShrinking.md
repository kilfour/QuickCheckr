# Deliberation Shrinking
Sometimes the shrinker comes up with a *local* minimal fail-case, not *the* minimal fail-case.  
Deliberation Shrinking allows us to instruct the Shrinker to explore the input further in a directed fasion.  
It is an opt-in feature, activated through the Configuration object.

Consider the following model:  
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
It's a simple bank account with a no-overdraft invariant and a fee applied on each transaction.  
We can verify this with the following Checkr:  
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int(1, 10))
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
Now if we run this we might get something like this:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:65:1
 Original failing run:    3 executions
 Seed:                    1920827676
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 8
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 1
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 5
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 2x
 ------------------------------------------------------------
```
As you can see, this is a *local* minimal fail-case, but a smaller one exists.  

Let's configure a *DeliberationPolicy*, looking for a fail-case with less executions:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = (a, b) => a.Verdict!.ExecutionCount < b.Verdict!.ExecutionCount ? a : b };
```
Running this test now, with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.  
```text
------------------------------------------------------------
 Test:                    RunTest_Deliberated
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:110:1
 Original failing run:    2 executions
 Seed:                    679678924
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 2
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 1
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 1x
 ------------------------------------------------------------
```
