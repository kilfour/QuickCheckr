# Multiple Relevant Properties
```csharp
    from standIn in Trackr.StandIn(123)
    from obj in Checkr.Input("BaggyInters", Fuzzr.One<BaggyInters>())
    from trace in Checkr.Trace("Original", () => obj)
    from spec in Checkr.Spec("obj.IntOne == 1 || obj.IntThree == 1",
        () => obj.IntOne != 1 && obj.IntThree != 1)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\F_MultipleRelevantProperties.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1889365782
 ------------------------------------------------------------
  Executed:
   - Input: BaggyInters = { IntThree: 1 }
   - Trace: Original = { IntOne: 1, IntTwo: 69, IntThree: 1 }
 ========================================================
  !! Spec Failed: obj.IntOne == 1 || obj.IntThree == 1
 ========================================================
```
