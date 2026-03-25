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
  Difference must not be small.
  Seed: 1449083695
 ------------------------------------------------------------
  Falsified:
    Input = { A: 37, B: 38 }
    Redux = { A: 10, B: 6 }

  Original:
    { A: 37, B: 38 }
 ------------------------------------------------------------
```
