# A Collection Of Objects

**The Model:**  
```csharp
public class Thing
{
    public int One { get; set; }
    public int Two { get; set; }
}
```

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.One<Thing>().Many(2, 5))
from show in Showr.ForTrace(ListShowr)
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("Only answers for One",
    () => list.Any(), () => list.All(a => a.One != 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                K_AListOfObjects.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    849984174
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ { One: 42 } ]
   - Trace: Original = [
    { One: 13, Two: 49 },
    { One: 61, Two: 79 },
    { One: 93, Two: 91 },
    { One: 18, Two: 39 },
    { One: 42, Two: 54 }
]
 ===============================================
  !! Expectation Failed: Only answers for One
 ===============================================
```
