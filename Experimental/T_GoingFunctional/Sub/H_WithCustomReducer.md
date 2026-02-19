# With Custom Reducer

**The Invariant:**  
```csharp
Check.ForAll(257911700, Fuzzr.Int(0, 10), a => a != 6, Reduce.Values([0, 6]));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H_WithCustomReducer.cs:24:1
 Original failing run:    1 execution
 Seed:                    257911700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
