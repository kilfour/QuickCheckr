# Applies Default Reducers

**The Invariant:**  
```csharp
Check.ForAll(1429680296, Fuzzr.Int(0, 10), a => a < 2);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                F_AppliesDefaultReducers.cs:25:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1429680296
 ------------------------------------------------------------
  Executed:
   - Input: Input = 5
            Redux   2
 ===================================
  !! Expectation Failed: Property
 ===================================
```
