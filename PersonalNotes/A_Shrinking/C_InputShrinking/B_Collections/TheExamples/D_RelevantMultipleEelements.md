# Relevant Multiple Eelements
```csharp
    from input in Checkr.Input("input", Fuzzr.Int(1, 10).Many(1, 10))
    from spec in Checkr.SpecIf("spec", () => input.Count() > 2, () =>
        !(input.ToList()[0] == 1
        && input.ToList()[1] == 2
        && input.ToList()[2] == 3))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\D_RelevantMultipleEelements.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    500957855
 ------------------------------------------------------------
  Executed:
   - Input: input = [ 1, 2, 3 ]
 ========================
  !! Spec Failed: spec
 ========================
```
