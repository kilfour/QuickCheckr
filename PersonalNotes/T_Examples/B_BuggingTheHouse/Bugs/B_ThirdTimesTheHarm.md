# Third Times the Harm
```csharp
public class BugHouse
{
    private int count;
    public bool Run(int a)
    {
        if (a == 6) count++;
        return !(count == 3);
    }
}
```
Throws as soon as the value 6 has been seen three times.  
A straightforward counter-based failure condition.  
```csharp
    from a in Checkr.Input("a", Fuzzr.Int())
    from bughouse in Trackr.Stashed(() => new BugHouse())
    from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
    from spec in Checkr.Spec("Returns true", () => output)
    select Case.Closed;
```
## Minimal Fail Case:
- **Length:** 3 executions.
- **Example input:** `[6, 6, 6]`
```csharp
count = 0
Run( a = 6 ) // count = 1
Run( a = 6 ) // count = 2
Run( a = 6 ) // count == 3 → fail
```  
## Report:
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
