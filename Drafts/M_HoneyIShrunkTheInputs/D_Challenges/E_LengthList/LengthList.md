# Length List

A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Length List", TheFuzzr,
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The maximum value of the list is smaller than 900.",
    () => input.Max() < 900)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LengthList.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    1834085712
 ------------------------------------------------------------
  Executed:
   - Input: Length List = [ 962 ]
            Redux         [ 900 ]
   - Trace: Original    = [ 974, 962, 872, 681, 543, 118, 578 ]
 ============================================================================
  !! Expectation Failed: Maximum value of the list is at smaller than 900.
 ============================================================================
```
