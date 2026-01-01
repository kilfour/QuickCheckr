# Relevant Item Is All But Default
Just making sure `default` values, `0` in this case, get handled correctly.  
**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(2, 10))
from expect in Checkr.Expect("List contains only 0", () => listInput.All(a => a == 0))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                E_RelevantItemIsAllButDefault.cs:46:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    816312044
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 51 ]
   - Trace: List Input = [ 28, 0, 50, 27, 37, 54, 74, 56, 51 ]
 ===============================================
  !! Expectation Failed: List contains only 0
 ===============================================
```
