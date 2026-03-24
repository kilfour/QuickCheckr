# Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Testr:**  
```csharp
Testr.Named("The sum of all values is less than 5 * 256.")
    .For(Fuzzr.Tuple(ShortList, ShortList, ShortList, ShortList, ShortList))
    .Deliberate(H.ElementCount, 2)
    .Assert(a => H.Sum(a) < (5 * 256));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Bound5.cs:30:1
 Original failing run:    1 execution
 Seed:                    472166887
 ------------------------------------------------------------
  Executed:
   - Input    = ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
     Redux      ( [ ], [ ], [ ], [ -7527 ], [ -25242 ] )
   - Original = ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
 ======================================================================
  !! Expectation Failed: The sum of all values is less than 5 * 256.
 ======================================================================
```
