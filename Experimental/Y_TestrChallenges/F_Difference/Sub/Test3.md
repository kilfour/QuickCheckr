# Test3

Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference is not exactly 1.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) != 1);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference is not exactly 1.
  Seed: 1231462692
 ------------------------------------------------------------
  Falsified:
    Input = { A: 51, B: 52 }
    Redux = { A: 10, B: 9 }

  Original:
    { A: 51, B: 52 }
 ------------------------------------------------------------
```
