# Deletion

This tests the property "if we remove an element from a list, the element is
no longer in the list". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
  

**The Testr:**  
```csharp
Testr
    .Named("Element is no longer in the list.")
    .For(Fuzzr.Tuple(Fuzzr.Int(1, 10).Many(3, 20), Fuzzr.Int(1, 10)))
    .Assert(a =>
    {
        var list = a.Item1.ToList();
        list.Remove(a.Item2);
        return !list.Contains(a.Item2);
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Element is no longer in the list.
  Seed: 712389878
 ------------------------------------------------------------
  Falsified:
    Input = ( [ 1, 1 ], 1 )


  Original:
    ( [ 1, 1, 8, 3, 5, 1, 9, 1, 7, 2, 8, 6, 3 ], 1 )
 ------------------------------------------------------------
```
