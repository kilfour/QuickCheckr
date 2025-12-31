# Showr Use
```csharp
    from input in Checkr.Input("int", Fuzzr.Int())
    from showOne in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"One: {a}"))
    from traceOne in Checkr.Trace("int One", () => input)
    from showTwo in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"Two: {a}"))
    from traceTwo in Checkr.Trace("int Two", () => input)
    from noAnswer in Checkr.Expect("no answer", () => input != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\D_ShowrUse\ShowrUse.cs:20:1
 Original failing run:    1 execution
 Seed:                    553690512
 ------------------------------------------------------------
  Executed:
   - Input: int     = 42
   - Trace: int One = One: 42
   - Trace: int Two = Two: 42
 ====================================
  !! Expectation Failed: no answer
 ====================================
```
