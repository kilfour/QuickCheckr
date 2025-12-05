# Nested Collections
```csharp
    from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.SpecIf("List[0][0] is not 42",
        () => input.Any() && input.First().Any(),
        () => input.First().First() != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\F_NestedCollections.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    146712596
 ------------------------------------------------------------
  Executed:
   - Input: List = [ [ 42 ] ]
   - Trace: List = [ [ 42, 1, 15 ], [ 17 ], [ 27, 9 ] ]
 ========================================
  !! Spec Failed: List[0][0] is not 42
 ========================================
```
