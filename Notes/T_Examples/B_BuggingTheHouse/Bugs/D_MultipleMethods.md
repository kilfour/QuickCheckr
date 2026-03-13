# Multiple Methods
```csharp
public class BugHouse
{
    private string? bug;
    public bool RunInt(int a)
    {
        bug += "1";
        if (bug.EndsWith("1221") && a == 6) return false;
        return true;
    }
    public bool RunString(string a)
    {
        bug += "2";
        if (bug.EndsWith("122") && a == "p") return false;
        return true;
    }
}
```
Exception occurs only from a specific interleaving of RunInt and RunString calls, each appending different markers to state.
Demonstrates multi-method state interaction.  
```csharp
from bughouse in Trackr.Stashed(() => new BugHouse())
from operations in Checkr.OneOf(
    // --
    from i in Checkr.Input("Int", Fuzzr.Int())
    from output in Checkr.Act("BugHouse.RunInt", () => bughouse.RunInt(i))
    from expect in Checkr.Expect("RunInt s true", () => output)
    select Case.Closed,
    // --
    from str in Checkr.Input("String", Fuzzr.String())
    from output in Checkr.Act("BugHouse.RunString", () => bughouse.RunString(str))
    from expect in Checkr.Expect("RunString s true", () => output)
    select Case.Closed)
    //--
select Case.Closed;
```
## Minimal Fail Case (one example):
- **Length:** 4 executions.
- **Example input sequence:**
```csharp
RunInt(<any>)        // bug = "1"
RunString(<any>)     // bug = "12"
RunString(<any>)     // bug = "122"
RunInt(6)            // bug ends with "1221" and `a == 6` → fail
```  
## Reports:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\D_MultipleMethods.cs:34:1
 Original failing run:    59 executions
 Minimal failing case:    4 executions (after 58 shrinks)
 Seed:                    1292206393
 ------------------------------------------------------------
  Executed: BugHouse.RunInt
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: BugHouse.RunString (2 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: BugHouse.RunInt
   - Input: Int = 6
 ==============================================
  !! Expectation Failed: RunInt returns true
 ==============================================
 Passed Expectations
 - RunString returns true: 34x
 - RunInt returns true: 24x
 ------------------------------------------------------------
```
