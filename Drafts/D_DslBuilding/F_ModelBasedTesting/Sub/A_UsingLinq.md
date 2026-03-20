# Using Linq

**The Checkr:**  
```csharp
from model in Trackr.Stashed(() => new CalculatorModel())
from sut in Trackr.Stashed(() => new Calculator())
from ops in Checkr.OneOf(
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Add", () =>
    {
        model.Add(input);
        sut.Add(input);
    })
    select Case.Closed,
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Subtract", () =>
    {
        model.Subtract(input);
        sut.Subtract(input);
    })
    select Case.Closed,
    Checkr.Act("Clear", () =>
    {
        model.Clear();
        sut.Clear();
    })
)
from observe in Checkr.Expect("Observe", () => model.Result == sut.Result)
from trace in Checkr.TraceWhen("Observation", () => model.Result != sut.Result, () => $"Model: {model.Result}, Sut: {sut.Result}.")
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_UsingLinq.cs:72:1
 Original failing run:    14 executions
 Minimal failing case:    4 executions (after 13 shrinks)
 Seed:                    1299945702
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 196.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 13x
 ------------------------------------------------------------
```
