# There Is Always An Account
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
    from amount in Checkr.Input("Amount", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from trace in Checkr.Trace("Amount", () => amount)
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
## The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:25:1
 Original failing run:    5 executions
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 1
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 4x
 ------------------------------------------------------------
```
## Original Run:
```text
------------------------------------------------------------
 Test:                    FullRunTest
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:50:1
 Original failing run:    5 executions
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed: Deposit
   - Trace: Amount = 94
 ------------------------------------------------------------
  Executed: Deposit
   - Trace: Amount = 59
 ------------------------------------------------------------
  Executed: Withdraw
   - Trace: Amount = 43
 ------------------------------------------------------------
  Executed: Withdraw
   - Trace: Amount = 63
 ------------------------------------------------------------
  Executed: Withdraw
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 4x
 ------------------------------------------------------------
```
