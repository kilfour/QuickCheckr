# Object Of Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Museum",
    from a in Fuzzr.Int().Many(2).ToList()
    from b in Fuzzr.Int().Many(2).ToList()
    from c in Fuzzr.Int().Many(0).ToList()
    select new Museum(a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("First list does not contain 42.", () => !input.A.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Q_ObjectOfLists.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    2082252864
 ------------------------------------------------------------
  Executed:
   - Input: Museum   = { A: [ 42 ], C: [ ] }
   - Trace: Original = { A: [ 42, 64 ], B: [ 84, 7 ], C: [ ] }
   - WARNING: Empty collection deemed irrelevant at input 'Museum'.
 ==========================================================
  !! Expectation Failed: First list does not contain 42.
 ==========================================================
```
