# Test1

Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be zero.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || a.A != a.B);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test1.cs:21:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1485535450
 ------------------------------------------------------------
  Executed: Run
   - Input    = { A: 93, B: 93 }
     Redux      { A: 10, B: 10 }
   - Original = { A: 93, B: 93 }
 =======================================================
  !! Expectation Failed: Difference must not be zero.
 =======================================================
```
