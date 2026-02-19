# With Classifiers Seeded Showing Redux Too

**The Invariant:**  
```csharp
Check.ForAll(893841622, Fuzzr.Int(0, 10), a => a == 6, Try.Default<int>(), Try.Values(6));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                G_WithClassifiersSeededShowingReduxToo.cs:24:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    893841622
 ------------------------------------------------------------
  Executed:
   - Input: Input = 9
            Redux   7
 ===================================
  !! Expectation Failed: Property
 ===================================
```
