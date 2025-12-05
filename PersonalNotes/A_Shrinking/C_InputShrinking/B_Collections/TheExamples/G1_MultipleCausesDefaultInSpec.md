# Multiple Causes Default In Spec
```csharp
    from substite in Trackr.StandIn(666)
    from input in Checkr.Input("Input", Fuzzr.Constant<IEnumerable<int>>([0, 2, 0]))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.SpecIf("input.Count() > 2, () => !input.Contains(0)",
        () => input.Count() > 2, () => !input.Contains(0))
    select Case.Closed;
```
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\G1_MultipleCausesDefaultInSpec.cs:21:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1238961515
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, 0 ]
   - Trace: List = [ 0, 2, 0 ]
 ===============================================================
  !! Spec Failed: input.Count() > 2, () => !input.Contains(0)
 ===============================================================
```
