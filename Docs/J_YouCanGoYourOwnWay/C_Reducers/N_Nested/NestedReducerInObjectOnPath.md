# Nested Reducer In Object On Path

**The Model:**  
```csharp
public class TheUniverse
{
    public int AnswerOne { get; set; }
    public int AnswerTwo { get; set; }
    public int AnswerThree { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(),
    [("Universe.AnswerOne", Reduce.TowardsZero), ("Universe.AnswerTwo", Reduce.Towards(100))])
from trace in Checkr.Trace("Universe", () => universe)
from justRight in Checkr.Expect("just right", () => universe.AnswerOne <= 42 || universe.AnswerTwo >= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedReducerInObjectOnPath.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1118829238
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { AnswerOne: 91, AnswerTwo: 15 }
            Redux      { AnswerOne: 43, AnswerTwo: 41 }
   - Trace: Universe = { AnswerOne: 91, AnswerTwo: 15, AnswerThree: 52 }
 =====================================
  !! Expectation Failed: just right
 =====================================
```
