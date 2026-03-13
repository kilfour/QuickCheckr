# A Stateful List
**The Model:**  
```csharp
public class ListWrap
{
    private readonly List<int> list = [];
    public IReadOnlyList<int> List => list;
    public void Add(int item) => list.Add(item);
    public void Remove(int item) => list.Remove(item);
}
```
**The Checkr:**  
```csharp
from listWrap in Trackr.Stashed(() => new ListWrap())
from item in Checkr.Input("Item", Fuzzr.Int(0, 10))
from traceBefore in Checkr.Trace("Before", () => listWrap.List)
from operations in Checkr.OneOf(
    from add in Checkr.Act("Add", () => listWrap.Add(item))
    from listContainsItem in Checkr.Expect("List Contains Item",
        () => listWrap.List.Contains(item))
    select Case.Closed,
    from remove in Checkr.Act("Remove", () => listWrap.Remove(item))
    from listDoesNotContainItem in Checkr.Expect("List Does Not Contain Item",
        () => !listWrap.List.Contains(item))
    select Case.Closed)
from traceAfter in Checkr.Trace("After", () => listWrap.List)
select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Notes\T_Examples\C_AStatefulList\AStatefulList.cs:32:1
 Original failing run:    9 executions
 Minimal failing case:    3 executions (after 6 shrinks)
 Seed:                    254875886
 ------------------------------------------------------------
  Executed: Add
   - Input: Item   = 3
   - Trace: Before = [ ]
   - Trace: After  = [ 3 ]
 ------------------------------------------------------------
  Executed: Add
   - Input: Item   = 3
   - Trace: Before = [ 3 ]
   - Trace: After  = [ 3, 3 ]
 ------------------------------------------------------------
  Executed: Remove
   - Input: Item   = 3
   - Trace: Before = [ 3, 3 ]
   - Trace: After  = [ 3 ]
 =====================================================
  !! Expectation Failed: List Does Not Contain Item
 =====================================================
 Passed Expectations
 - List Does Not Contain Item: 4x
 - List Contains Item: 4x
 ------------------------------------------------------------
```
