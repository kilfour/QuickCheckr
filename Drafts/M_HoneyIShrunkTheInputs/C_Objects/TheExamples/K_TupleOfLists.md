# Tuple Of Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Tuple",
    from a in Fuzzr.Int().Many(2)
    from b in Fuzzr.Int().Many(2)
    from c in Fuzzr.Int().Many(0)
    select (a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("First list does not contain 42.", () => !input.a.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                K_TupleOfLists.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1892439403
 ------------------------------------------------------------
  Executed:
   - Input: Tuple    = ( [ 42 ], _, [ ] )
   - Trace: Original = ( [ 42, 32 ], [ 17, 69 ], [ ] )
   - WARNING: Empty collection deemed irrelevant at input 'Tuple'.
 ==========================================================
  !! Expectation Failed: First list does not contain 42.
 ==========================================================
```
