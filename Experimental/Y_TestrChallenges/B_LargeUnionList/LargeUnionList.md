# Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Testr:**  
```csharp
Testr.Named("No more than four distinct integers.")
    .For(Fuzzr.Int().Many(0, 10).ToList().Many(5, 20).ToList())
    .Deliberate(a => 0 - a.Count, -5)
    .Assert(a => a.SelectMany(a => a).Distinct().Count() <= 4);
```

**The Report:**  
```text
------------------------------------------------------------
  No more than four distinct integers.
  Seed: 1575924946
 ------------------------------------------------------------
  Falsified:
    Input = [ [ _, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
    Redux = [ [ _, 0 ], [ 1 ], [ 2 ], [ 3 ] ]

  Original:
    [ [ 91, 42, 27, 37, 85, 85, 40, 91, 75 ], [ 43, 58, 67 ], [ 78, 1, 82, 71, 59, 69, 81, 5 ], [ 56, 7, 12, 35 ], [ 74, 37, 40, 98, 14, 28 ], [ 26, 71, 93, 9, 98, 27 ], [ ], [ 78, 39, 94, 60 ], [ 52, 60 ], [ 30, 10, 9, 9, 87, 99 ], [ 30, 74, 50, 82, 18, 38, 93 ], [ 45, 40, 18, 52 ], [ 89, 62, 60 ], [ 79, 61, 63, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
 ------------------------------------------------------------
```
