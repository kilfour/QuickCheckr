# Nested In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from answer in Checkr.Input("Answer", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from universe in Checkr.Capture(() => new TheUniverse { Answer = answer })
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1930190827
 ------------------------------------------------------------
  Executed:
   - Input: Answer = 96
            Redux    43
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
