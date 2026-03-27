# Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Heap", TheFuzzr)
from correct in Checkr.Act("Correct", () => Flatten(input).OrderBy(x => x).ToList())
from buggy in Checkr.Act("Buggy", () => Flatten(input).ToList())
from expect in Checkr.Expect("Heap sort is correct", () => correct.SequenceEqual(buggy))
from trace in Checkr.Trace("Original", () => input)
select Case.Closed;
```

**The Fuzzr:**  
```csharp
from depth in Configr<Heap>.Depth(3, 20)
from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
from terminator in Configr<Heap>.EndOn<Empty>()
from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
from value in Configr<Node>.Property(a => a.Value,
    from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
from heap in Fuzzr.One<Heap>()
select heap
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
       (caseFile.InputCount == 0 ? 100 : 0) +
       caseFile.InputsNamed<Heap>("Heap", a => Flatten(a).Count()),
    ReportMode = config.ReportMode & ~ReportMode.Warning
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                BinHeap.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    195567586
 ------------------------------------------------------------
  Executed: Buggy, Correct
   - Input: Heap     = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
   - Trace: Original = { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ===============================================
  !! Expectation Failed: Heap sort is correct
 ===============================================
```
