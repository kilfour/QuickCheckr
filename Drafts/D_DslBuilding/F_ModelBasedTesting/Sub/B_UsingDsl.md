# Using Dsl

**The Checkr:**  
```csharp
Check
    .Model(() => new CalculatorModel())
    .Sut(() => new Calculator())
    .Operation("Add", Fuzzr.Int(),
        (model, a) => model.Add(a),
        (sut, a) => sut.Add(a))
    .Operation("Subtract", Fuzzr.Int(),
        (model, a) => model.Subtract(a),
        (sut, a) => sut.Subtract(a))
    .Operation("Clear",
        model => model.Clear(),
        sut => sut.Clear())
    .Report((model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.")
    .Observe((model, sut) => model.Result == sut.Result);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_UsingDsl.cs:50:1
 Original failing run:    7 executions
 Minimal failing case:    4 executions (after 6 shrinks)
 Seed:                    1879928073
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 193.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 6x
 ------------------------------------------------------------
```
