# Nested Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 10)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("Nested List", a => a.Count),
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedLists.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 29 shrinks)
 Seed:                    1023940145
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _, _, _, _, _, _, _, _, _, _ ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
