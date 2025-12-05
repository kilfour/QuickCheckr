# Array Shrinking
```csharp
var array = new[] { 42, 2, 3 };
var check =
    from input in Checkr.Input("int[]", Fuzzr.Constant(array))
    from spec in Checkr.SpecIf("int[0] is not 42",
        () => input.Length != 0,
        () => input.First() != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\T_ArrayShrinking.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1100967994
 ------------------------------------------------------------
  Executed:
   - Input: int[] = [ 42 ]
 ====================================
  !! Spec Failed: int[0] is not 42
 ====================================
```
