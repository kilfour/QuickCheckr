# Nested Reducer In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(), Shrink.OnType(Reduce.TowardsZero))
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedReducerInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    686899245
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { Answer: 61 }
            Redux      { Answer: 43 }
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
