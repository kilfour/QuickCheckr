# Relevant Element And Size
```csharp
    from input in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>([666, 42]))
    from spec in Checkr.SpecIf("Second element is not 42", () => input.Count() > 1, () => input.ElementAt(1) != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\E_RelevantElementAndSize.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1058309953
 ------------------------------------------------------------
  Executed:
   - Input: List = [ _, 42 ]
 ============================================
  !! Spec Failed: Second element is not 42
 ============================================
```
