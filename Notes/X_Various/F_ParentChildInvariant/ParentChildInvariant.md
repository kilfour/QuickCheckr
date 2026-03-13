# Parent Child Invariant

**The Model:**  
```csharp
record Account(int Balance, int CreditLimit);
```

**The Checkr:**  
```csharp
from account in Checkr.Input("pair", Fuzzr.Constant(new Account(-100, 50)))
from expect in Checkr.Expect("Balance >= -CreditLimit (i.e. never exceeds credit)",
     () => account.Balance >= -account.CreditLimit)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ParentChildInvariant.cs:54:1
 Original failing run:    1 execution
 Seed:                    483645165
 ------------------------------------------------------------
  Executed:
   - Input: pair = { Balance: -100, CreditLimit: 50 }
 ==============================================================================
  !! Expectation Failed: Balance >= -CreditLimit (i.e. never exceeds credit)
 ==============================================================================
```
