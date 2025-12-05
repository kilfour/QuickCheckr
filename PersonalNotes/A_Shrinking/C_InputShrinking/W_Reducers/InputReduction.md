# Input Reduction
```csharp
    from i in Checkr.Input("Int", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from spec in Checkr.Spec("Int should be smaller than 42", () => i < 42)
    from trace in Checkr.Trace("Int", () => i)
    select Case.Closed;
```
```csharp
private static IEnumerable<int> TowardsZero(int input)
{
    var local = input;
    while (local != 0)
    {
        if (local > 0)
            yield return --local;
        else
            yield return ++local;
    }
}
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\W_Reducers\InputReduction.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Int = 42
   - Trace: Int = 79
 =================================================
  !! Spec Failed: Int should be smaller than 42
 =================================================
```
