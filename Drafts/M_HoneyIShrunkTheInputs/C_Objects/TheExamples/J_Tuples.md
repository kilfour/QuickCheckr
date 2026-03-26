# Tuples

**The Checkr:**  
```csharp
from input in Checkr.Input("Tuple",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    from c in Fuzzr.Int()
    select (a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("Sum is < 42.", () => input.a + input.b < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J_Tuples.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    751254928
 ------------------------------------------------------------
  Executed:
   - Input: Tuple    = ( 52, 25, _ )
            Redux      ( 17, 25, _ )
   - Trace: Original = ( 52, 25, 23 )
 =======================================
  !! Expectation Failed: Sum is < 42.
 =======================================
```
