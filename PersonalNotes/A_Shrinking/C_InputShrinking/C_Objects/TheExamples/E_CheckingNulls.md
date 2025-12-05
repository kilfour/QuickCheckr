# Checking Nulls
## Catching a Null Reference Exception
Any exception thrown by the Checkr will be caught and considered a failure.  
```csharp
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
 Location:                PersonalNotes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:22:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    358877731
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 78 } }
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in PersonalNotes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 131
   at QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass2_1.GetCheckr() in PersonalNotes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 54
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 74
 ===========================================================================
```
But, when `Irrelevant` is `null`, QuickCheckr can not shrink.  
```text
------------------------------------------------------------
 Test:                    RunTest_NotShrinkingNull
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:65:1
 Original failing run:    1 execution
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null, Irrelevant: null }
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 131
   at QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass2_1.GetCheckr() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 54
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 37
 ===========================================================================
```
The current workaround to avoid the irrelevant input reporting involves helping the shrinker by rewriting your checkr.  
```csharp
    from everything in Checkr.Input("Everything", Fuzzr.One<Life>().NullableRef(), a => a with { FuzzrShrinker = 10 })
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.One<Life>().NullableRef(), a => a with { FuzzrShrinker = 10 })
    from universe in Trackr.Capture(() => new TheUniverse { Everything = everything, Irrelevant = irrelevant })
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Alternative
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:93:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: Everything = null
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 131
   at QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass7_2.GetCheckr_Alternative() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 123
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 37
 ===========================================================================
```
