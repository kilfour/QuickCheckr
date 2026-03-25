# Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Testr:**  
```csharp
Testr.Named("Heap sort is correct.")
    .For(TheFuzzr)
    .Deliberate(a => Flatten(a).Count())
    .Assert(a =>
    {
        var correct = Flatten(a).OrderBy(x => x).ToList();
        var buggy = Flatten(a).ToList();
        return correct.SequenceEqual(buggy);
    });
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

**The Report:**  
```text
------------------------------------------------------------
  Heap sort is correct.
  Seed: 2136249593
 ------------------------------------------------------------
  Falsified:
    Input = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
    Redux = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 0, Left: { }, Right: { } } }

  Original:
    { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ------------------------------------------------------------
```
