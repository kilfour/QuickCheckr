# Removing Irrelevant Items
Consider a test that forbids the value `7`.  
**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
from expect in Checkr.Expect("List does not contain 7", () => !listInput.Contains(7))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_RemovingIrrelevantItems.cs:46:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1833095082
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 7 ]
   - Trace: List Input = [ 69, 49, 90, 36, 16, 71, 95, 7, 84 ]
 ==================================================
  !! Expectation Failed: List does not contain 7
 ==================================================
```
Even if the original list was long and varied, QuickCheckr isolates the single relevant element:  
