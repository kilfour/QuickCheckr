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
from input in Checkr.Input("Bound5", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of all values is less than 5 * 256.",
    () => Sum(input) < (5 * 256))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<(List<short>, List<short>, List<short>, List<short>, List<short>)>(
            "Bound5", a => ElementCount(a)),
    DeliberationTarget = 2
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Bound5.cs:32:1
 Original failing run:    1 execution
 Seed:                    1866106578
 ------------------------------------------------------------
  Executed:
   - Input: Bound5   = ( _, [ -21169 ], _, [ -13193 ], _ )
   - Trace: Original = ( [ ], [ -8290, -21169 ], [ ], [ -13193 ], [ ] )
 ======================================================================
  !! Expectation Failed: The sum of all values is less than 5 * 256.
 ======================================================================
```
