# Convoluted Countdown
```csharp
public class BugHouse
{
	private int count;
	public bool Run(int a)
	{
		if (a == 6 && count != 3) count++;
		if (count >= 3) count++;
		if (count == 5) return false; return true;
	}
}
```
Fails after a more complex sequence: specific inputs increment the counter in different ways, 
requiring a particular input dance to trigger the failure.  
```csharp
from a in Checkr.Input("a", Fuzzr.Int())
from bughouse in Trackr.Stashed(() => new BugHouse())
from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
from spec in Checkr.Spec("Returns true", () => output)
select Case.Closed;
```
## Minimal Fail Case:
- **Length:** 4 executions.
- **Example input:** `[6, 6, 6, 42]`
```csharp
count = 0
Run( a = 6 ) // +1 → 1
Run( a = 6 ) // +1 → 2
Run( a = 6 ) // +1 → 3, then >=3 → +1 → 4
Run( a != 6 ) // skip first inc, >=3 → +1 → 5 → fail
```  
## Reports:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\B_ThirdTimesTheHarm.cs:34:1
 Original failing run:    36 executions
 Minimal failing case:    3 executions (after 33 shrinks)
 Seed:                    766917706
 ------------------------------------------------------------
  Executed: BugHouse.Run (3 Times)
   - Input: a = 6
 ================================
  !! Spec Failed: Returns true
 ================================
 Passed Specs
 - Returns true: 35x
 ------------------------------------------------------------
```
