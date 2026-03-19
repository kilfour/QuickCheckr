# Collection Is Null

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>(null!))
from expect in Checkr.Expect("Always fails.", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                P_CollectionIsNull.cs:32:1
 Original failing run:    1 execution
 Seed:                    849984174
 ------------------------------------------------------------
  Executed:
   - Input: List = null
   - WARNING: Null value for type 'IEnumerable`1' at input 'List'.
 ========================================
  !! Expectation Failed: Always fails.
 ========================================
```
