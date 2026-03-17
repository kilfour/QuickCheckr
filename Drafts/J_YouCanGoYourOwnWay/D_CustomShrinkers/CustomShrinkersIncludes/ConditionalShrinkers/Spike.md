# Spike

**The Checkr:**  
```csharp
from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 5)),
    Shrink.When<int>(
        s => s.Value(a => a == 5),
        s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("Original", () => inter)
from expect in Checkr.Expect("All ints smaller than 1.", () => inter.A < 1 && inter.B < 1)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Spike.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1645367809
 ------------------------------------------------------------
  Executed:
   - Input: Inter    = { B: 5 }
            Redux      { B: 1 }
   - Trace: Original = { A: 42, B: 5 }
 ===================================================
  !! Expectation Failed: All ints smaller than 1.
 ===================================================
```
