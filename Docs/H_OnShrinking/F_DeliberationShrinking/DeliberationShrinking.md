# Deliberation Shrinking
## The Model:
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
## The Checkr:
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int(1, 5))
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
## The Configuration:
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = (a, b) => a.Verdict!.ExecutionCount < b.Verdict!.ExecutionCount ? a : b };
```
## The Report:
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:47:1
 Original failing run:    2 executions
 Seed:                    1258846520
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
Running this test with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.  
