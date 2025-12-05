# The Element Remover
This test focuses on a simple faulty implementation:  
```csharp
public static class ElementRemover
{
    public static List<int> Remove(List<int> list, int item)
    {
        list.Remove(item);
        return list;
    }
}
```
`ElementRemover` only removes the *first* occurrence of a value in a list.  
Using one input, one action, and one specification,
the test captures the minimal failing case where the list contains multiple copies of the target element.  
```csharp
    from list in Checkr.Input("List", GetFuzzr())
    from element in Checkr.Input("Item to Remove", Fuzzr.Int(0, 10))
    from output in Checkr.Act("Delete from List",
        () => ElementRemover.Remove([.. list], element))
    from listDoesNotContainElement in Checkr.Spec(
        "List Does Not Contain Element",
        () => !output.Contains(element))
    from traceList in Checkr.Trace("List Before Shrinking", () => list)
    select Case.Closed;
```
QuickCheckr reduces the failing run to a list like `[ 5, 5 ]` with `5` as the element to remove,
exposing the bug clearly: the output still contains `5`.  
The generated report shows the input, the attempted deletion, a trace of the original list before shrinking and a trace of the result from the action.  
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\A_TheElementRemover\TheElementRemover.cs:31:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    438724296
 ------------------------------------------------------------
  Executed: Delete from List
   - Input: List = [ 5, 5 ]
   - Input: Item to Remove = 5
   - Trace: List Before Shrinking = [ 2, 8, 0, 5, 5, 1, 8, 2, 9 ]
 =================================================
  !! Spec Failed: List Does Not Contain Element
 =================================================
```
