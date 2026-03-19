# Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Distinct", Fuzzr.Int(1, 10).Many(3, 20),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen(
    "Contains at least three distinct elements.",
    () => input.Count() >= 3,
    () => input.ToHashSet().Count >= 3)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Distinct.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    767903368
 ------------------------------------------------------------
  Executed:
   - Input: Distinct = [ _, 8, 8 ]
   - Trace: Original = [ 5, 8, 8 ]
 =====================================================================
  !! Expectation Failed: Contains at least three distinct elements.
 =====================================================================
```
