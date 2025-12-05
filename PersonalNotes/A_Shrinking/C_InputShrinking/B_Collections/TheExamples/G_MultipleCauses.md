# Multiple Causes
```csharp
    from substite in Trackr.StandIn(666)
    from input in Checkr.Input("Input", Fuzzr.Constant<IEnumerable<int>>([1, 2, 1]))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.SpecIf("input.Count() > 2, () => !input.Contains(1)",
        () => input.Count() > 2, () => !input.Contains(1))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\G_MultipleCauses.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1238961515
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, 1 ]
   - Trace: List = [ 1, 2, 1 ]
 ===============================================================
  !! Spec Failed: input.Count() > 2, () => !input.Contains(1)
 ===============================================================
```
