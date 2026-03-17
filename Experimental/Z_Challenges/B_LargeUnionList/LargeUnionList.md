# Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("LargeUnionList", TheFuzzr,
        Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "No more than four distinct integers.",
    () => input.SelectMany(a => a).Distinct().Count() <= 4)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("LargeUnionList", a => 0 - a.Count),
    DeliberationTarget = -5
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LargeUnionList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 25 shrinks)
 Seed:                    1284201255
 ------------------------------------------------------------
  Executed:
   - Input: LargeUnionList = [ [ _ ], [ 98 ], [ 33 ], [ 41 ], [ 73 ] ]
            Redux            [ [ _ ], [ 74 ], [ 0 ], [ 1 ], [ 2 ] ]
   - Trace: Original       = [ [ 37, 88, 82, 64, 26, 15, 91, 65 ], [ 76, 17, 18, 41, 82, 29, 89, 80, 24 ], [ 74, 39, 69, 94, 76, 44, 14, 66 ], [ 32, 98 ], [ 16, 25, 11, 59, 67, 74, 9 ], [ 63, 55, 4 ], [ ], [ 96 ], [ 16, 42, 86 ], [ 92, 57, 74, 11, 38, 67 ], [ 7, 21, 13, 49, 72, 34, 88, 10, 1, 92 ], [ 24, 61, 16 ], [ 96, 12, 20, 36, 67, 80, 80 ], [ 98 ], [ 33, 73 ], [ ], [ 41 ], [ 73 ] ]
 ===============================================================
  !! Expectation Failed: No more than four distinct integers.
 ===============================================================
```
