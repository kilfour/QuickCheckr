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

**The Short List Fuzzr:**  
```csharp
Fuzzr.Short(short.MinValue, 256)
    .Many(0, 10)
    .Where(a => H.Sum(a) < 256)
    .ToList()
```

**The Helpers:**  
```csharp
public class H
{
    public static int ElementCount((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => a.Item1.Count + a.Item2.Count + a.Item3.Count + a.Item4.Count + a.Item5.Count;
    public static short Sum((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => (short)(Sum(a.Item1) + Sum(a.Item2) + Sum(a.Item3) + Sum(a.Item4) + Sum(a.Item5));
    public static short Sum(IEnumerable<short> list)
    {
        short sum = 0;
        foreach (var value in list)
            sum = (short)(sum + value);
        return sum;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
  The sum of all values is less than 5 * 256.
  Seed: 472166887
 ------------------------------------------------------------
  Falsified:
    Input = ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
    Redux = ( [ ], [ ], [ ], [ -7527 ], [ -25242 ] )

  Original:
    ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
 ------------------------------------------------------------
```
