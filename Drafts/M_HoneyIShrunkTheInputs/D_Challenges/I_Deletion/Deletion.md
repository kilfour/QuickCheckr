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
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Deletion", Fuzzr.Int(1, 10).Many(3, 20))
from toRemove in Checkr.Input("Element To Remove", () => Fuzzr.OneOf(input))
from removed in Checkr.Act("Remove Element", () =>
{
    var list = input.ToList();
    list.Remove(toRemove);
    return list;
})
from expect in Checkr.Expect("Element is no longer in the list", () => !removed.Contains(toRemove))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Deletion.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 16 shrinks)
 Seed:                    613616447
 ------------------------------------------------------------
  Executed: Remove Element
   - Input: Deletion          = [ 3, 3 ]
   - Input: Element To Remove = 3
 ===========================================================
  !! Expectation Failed: Element is no longer in the list
 ===========================================================
```
