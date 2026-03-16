# Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("ReverseList", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from reverse in Checkr.Act("Reverse List",
    () => { var list = new List<int>(input); list.Reverse(); return list; })
from expect in Checkr.Expect(
    "Reversing a list of integers results in the same list",
    () => reverse.SequenceEqual(input))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReverseList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    767678629
 ------------------------------------------------------------
  Executed: Reverse List
   - Input: ReverseList = [ _, 92 ]
   - Trace: Original    = [ 68, 89, 98, 88, 92 ]
 ================================================================================
  !! Expectation Failed: Reversing a list of integers results in the same list
 ================================================================================
```
