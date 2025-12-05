# When Relevant Item Is Default
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(0, 10))
    from spec in Checkr.Spec("List does not contain 0", () => !listInput.Contains(0))
    from trace in Checkr.Trace("List Input", () => listInput)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\A1_WhenRelevantItemIsDefault.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    820833320
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 0 ]
   - Trace: List Input = [ 84, 63, 90, 91, 55, 53, 39, 91, 0 ]
 ===========================================
  !! Spec Failed: List does not contain 0
 ===========================================
```
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(2, 10))
    from spec in Checkr.Spec("List contains only 0", () => listInput.All(a => a == 0))
    from trace in Checkr.Trace("List Input", () => listInput)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Negated
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\A1_WhenRelevantItemIsDefault.cs:57:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    816312044
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 51 ]
   - Trace: List Input = [ 28, 0, 50, 27, 37, 54, 74, 56, 51 ]
 ========================================
  !! Spec Failed: List contains only 0
 ========================================
```
