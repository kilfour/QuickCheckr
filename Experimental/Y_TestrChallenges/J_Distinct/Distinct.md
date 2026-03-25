# Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Testr:**  
```csharp
Testr
    .Named("Contains at least three distinct elements.")
    .For(Fuzzr.Int(1, 10).Many(3, 20))
    .Assert(a =>
    {
        if (a.Count() >= 3)
            return a.ToHashSet().Count >= 3;
        return true;
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Contains at least three distinct elements.
  Seed: 1943621438
 ------------------------------------------------------------
  Falsified:
    Input = [ 3, _, 3 ]


  Original:
    [ 3, 9, 3, 9, 3 ]
 ------------------------------------------------------------
```
