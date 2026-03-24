# Large Union List Testr

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Invariant:**  
```csharp
Testr.Named("No more than four distinct integers.")
    .For(Fuzzr.Int().Many(0, 10).ToList().Many(5, 20).ToList())
    // Prefer less lists, stop at 1
    .Deliberate(a => a.Count, 1)
    .Assert(a => a.SelectMany(a => a).Distinct().Count() <= 4)
    .Run(1_000.Runs());
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Z_LargeUnionListTestr.cs:38:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 11 shrinks)
 Seed:                    664059097
 ------------------------------------------------------------
  Executed:
   - Input    = [ [ _, 66, 42, 89, 25 ] ]
     Redux      [ [ _, 0, 1, 2, 3 ] ]
   - Original = [ [ ], [ 31, 86, 93, 91, 95 ], [ 19, 41, 94 ], [ ], [ 6, 48, 40, 26, 99, 87, 45, 68 ], [ 20, 29, 66, 42, 89, 25 ] ]
 ===============================================================
  !! Expectation Failed: No more than four distinct integers.
 ===============================================================
```
