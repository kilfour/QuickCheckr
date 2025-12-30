# The Showr Class
```csharp
    from acc in Trackr.Stashed(() => new Accumelator())
    from count in Checkr.Input("Counter", Fuzzr.Counter("count"))
    from show in Showr.ForInputs(() => Please.AllowMe().ToReplace<int>(a => $"Int {a}"))
    from a in Checkr.Input("a", Fuzzr.Int())
    from b in Checkr.Input("b", Fuzzr.Int())
    from add in Checkr.Act("Add", () => acc.Add(a, b))
    from notAnswerOnTwo in Checkr.Expect("not 42 on execution 2", () => !(count == 2 && acc.Total == 42))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\D_CheckrsCourt\C_TheShowrClass\TheShowrClass.cs:21:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    502142190
 ------------------------------------------------------------
  Executed: Add
   - Input: a = 11
   - Input: b = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Counter = 2
   - Input: a = 9
   - Input: b = 17
 ================================================
  !! Expectation Failed: not 42 on execution 2
 ================================================
 Passed Expectations
 - not 42 on execution 2: 1x
 ------------------------------------------------------------
```
