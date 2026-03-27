# Nested Collections Need To Grow
Adding a Fuzzr based classification is a quick way to verify that the collection was indeed irrelevant.

Adding a reducer as well, just to check whether the empty list also fails.  

**The Checkr:**  
```csharp
from fuzzr in Checkr.Capture(() => Fuzzr.Int().Many(3, 10))
from input in Checkr.Input("Museum",
    from a in fuzzr
    select new Museum(a),
    Shrink.OnType<IEnumerable<int>>(
        s => s.Classify(
            Try.Default<IEnumerable<int>>(),
            Try.Fuzzr(fuzzr)),
        s => s.Simplify(Reduce.Values<IEnumerable<int>>([[]]))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.A.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                N2_NestedCollectionsNeedToGrow.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    482835926
 ------------------------------------------------------------
  Executed:
   - Input: Museum   = { A: [ 74, 41, 59 ] }
            Redux      { A: [ ] }
   - Trace: Original = { A: [ 74, 41, 59 ] }
   - WARNING: No witness for reducer at 'Museum.A'.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
