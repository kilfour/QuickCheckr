# Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Bound5", TheFuzzr) // ,Shrink.OnType<short>(s => s.Simplify(() => new ReduceShort().TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of all values is less than 5 * 256.",
    () => input.Count == 5 && Sum(input) < 5 * 256)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Bound5.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1053663876
 ------------------------------------------------------------
  Executed:
   - Trace: Original = [ [ -8906 ], [ -14533, -5130, -30738, -29657, -2193, -5669, -4364, -337 ], [ -24159 ], [ ], [ ] ]
   - WARNING: All inputs were considered irrelevant.
   - WARNING: Empty collection deemed irrelevant at input 'Bound5'.
 ======================================================================
  !! Expectation Failed: The sum of all values is less than 5 * 256.
 ======================================================================
```
