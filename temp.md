# There Is Always An Account
## The Model:
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```
## The Checkr:
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from trace in Checkr.Trace("Amount", amount)
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
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:24:1
 Original failing run:    5 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed (5): Withdraw
   - Input: Amount = 1
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
```
## Original Run:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:24:1
 Original failing run:    5 executions
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed (1): Deposit
   - Trace: Amount = 94
 ------------------------------------------------------------
  Executed (2): Deposit
   - Trace: Amount = 59
 ------------------------------------------------------------
  Executed (3): Withdraw
   - Trace: Amount = 43
 ------------------------------------------------------------
  Executed (4): Withdraw
   - Trace: Amount = 63
 ------------------------------------------------------------
  Executed (5): Withdraw
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
```
