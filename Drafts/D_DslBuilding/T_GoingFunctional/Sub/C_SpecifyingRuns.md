# Specifying Runs

**The Invariant:**  
```csharp
Check.ForAll(200.Runs(), Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_SpecifyingRuns.cs:24:1
 Original failing run:    1 execution
 Seed:                    823515705
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
