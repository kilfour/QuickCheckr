# Using Dsl

**The Checkr:**  
```csharp
Check
    .Model(() => new Model())
    .Sut(() => new Sut())
    .Operation("Add", Fuzzr.Int(),
        (model, sut, a) =>
        {
            model.Add(a);
            sut.Add(a);
        })
    .Operation("Subtract", Fuzzr.Int(),
        (model, sut, a) =>
        {
            model.Subtract(a);
            sut.Subtract(a);
        })
    .Operation("Clear",
        (model, sut) =>
        {
            model.Clear();
            sut.Clear();
        })
    .Observe(
        (model, sut) => model.Result == sut.Result,
        (model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.");
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_UsingDsl.cs:72:1
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
