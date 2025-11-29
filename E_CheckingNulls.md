# Checking Nulls
## Catching a Null Reference Exception
Any exception thrown by the Checkr will be caught and considered a failure.  
```csharp
    // TODO make NullableRef the default for object? props in QuickFuzzr
    from universe in Checkr.Input("The Universe",
        from everything in Configr<TheUniverse>.Property(a => a.Everything, Fuzzr.One<Life>().NullableRef())
        from irrelevant in Configr<TheUniverse>.Property(a => a.Irrelevant, Fuzzr.One<Life>().NullableRef())
        from one in Fuzzr.One<TheUniverse>()
        select one)
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
        // No Spec == does not throw, implicitely
    select Case.Closed;
```
Note the shrinking of the `Irrelevant` property.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:22:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    358877731
 ------------------------------------------------------------
  Executed (1): The Answer
   - Input: The Universe = { Everything : null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 78 } }
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 54
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass2_1.GetCheckr() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 45
   at QuickCheckr.Checkr.<>c__DisplayClass5_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 32
 ===========================================================================
```
But, when `Irrelevant` is `null`, QuickCheckr can not shrink.  
```text
------------------------------------------------------------
 Test:                    RunTest_NotShrinkingNull
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:67:1
 Original failing run:    1 execution
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed (1): The Answer
   - Input: The Universe = { Everything : null, Irrelevant : null }
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 92
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass2_1.GetCheckr() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 56
   at QuickCheckr.Checkr.<>c__DisplayClass5_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 32
 ===========================================================================
```
