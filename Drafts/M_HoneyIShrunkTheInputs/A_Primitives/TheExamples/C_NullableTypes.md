# Nullable Types

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Constant<Nullable<int>>(null!))
from expect in Checkr.Expect("Fails", () => input.HasValue)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_NullableTypes.cs:32:1
 Original failing run:    1 execution
 Seed:                    393010076
 ------------------------------------------------------------
  Executed:
   - Input: Input = null
 ================================
  !! Expectation Failed: Fails
 ================================
```
