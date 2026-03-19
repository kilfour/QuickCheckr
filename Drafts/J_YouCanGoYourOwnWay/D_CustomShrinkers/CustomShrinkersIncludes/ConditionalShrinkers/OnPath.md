# On Path

**The Checkr:**  
```csharp
from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 21)),
    Shrink.When<int>(
        s => s.As<Inter>(a => a.A).Target(),
        s => s.Simplify(Reduce.Towards(3))),
    Shrink.When<int>(
        s => s.As<Inter>(a => a.B).Target(),
        s => s.Simplify(Reduce.Towards(0))))
from expect in Checkr.Expect("Sum smaller than 11.", () => inter.A + inter.B < 11)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                OnPath.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    776489069
 ------------------------------------------------------------
  Executed:
   - Input: Inter = { A: 42, B: 21 }
            Redux   { A: 3, B: 8 }
   - WARNING: No witness for reducer at 'Inter.A'.
 ===============================================
  !! Expectation Failed: Sum smaller than 11.
 ===============================================
```
