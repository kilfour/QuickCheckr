# Test2

Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be small.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) > 4 || a.A == a.B);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2.cs:28:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1449083695
 ------------------------------------------------------------
  Executed: Run
   - Input    = { A: 37, B: 38 }
     Redux      { A: 10, B: 6 }
   - Original = { A: 37, B: 38 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
