# Notes
A temporary collection of documented tests to be moved into the documentation later.  
## Shrinking
Whenever QuickCheckr comes accross a failure during a run, 
it tries to shrink the failing case to find a minimal failing case.

During this process no further executions are performed and no new random values are generated.

QuickCheckr then reports the minimal failing case, 
together with the number of executions, relevant actions and inputs that were required to find it.  
#### Enums
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<DayOfWeek>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<DayOfWeek>())
    from spec in Checkr.Expect("Should not be Monday", () => inputTwo != DayOfWeek.Monday)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\EnumShrinkingTests.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1732064051
 ------------------------------------------------------------
  Executed:
   - Input: Input Two = Monday
 ===============================================
  !! Expectation Failed: Should not be Monday
 ===============================================
```
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<SoundCheck>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<SoundCheck>())
    from spec in Checkr.Expect("Should not be Two", () => inputTwo != SoundCheck.Two)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Custom
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\EnumShrinkingTests.cs:55:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1205305161
 ------------------------------------------------------------
  Executed:
   - Input: Input Two = Two
 ============================================
  !! Expectation Failed: Should not be Two
 ============================================
```
##### Null String
**The Checkr:**  
```csharp
    from input in Checkr.Input("Input", Fuzzr.String())
    from spec in Checkr.Expect("Fails", () => input.Length > 0 && false)
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\A_NullString\NullString.cs:31:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    393010076
 ------------------------------------------------------------
  Executed
 ================================
  !! Expectation Failed: Fails
 ================================
```
#### Collections
##### When Relevant Item Is Default
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(0, 10))
    from spec in Checkr.Expect("List does not contain 0", () => !listInput.Contains(0))
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
 ==================================================
  !! Expectation Failed: List does not contain 0
 ==================================================
```
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(2, 10))
    from spec in Checkr.Expect("List contains only 0", () => listInput.All(a => a == 0))
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
 ===============================================
  !! Expectation Failed: List contains only 0
 ===============================================
```
##### Relevant Multiple Elements
```csharp
    from input in Checkr.Input("input", Fuzzr.Int(1, 10).Many(1, 10))
    from spec in Checkr.ExpectWhen("spec", () => input.Count() > 2, () =>
        !(input.ToList()[0] == 1
        && input.ToList()[1] == 2
        && input.ToList()[2] == 3))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\D_RelevantMultipleElements.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    500957855
 ------------------------------------------------------------
  Executed:
   - Input: input = [ 1, 2, 3 ]
 ===============================
  !! Expectation Failed: spec
 ===============================
```
##### Relevant Element And Size
```csharp
    from input in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>([666, 42]))
    from spec in Checkr.ExpectWhen("Second element is not 42", () => input.Count() > 1, () => input.ElementAt(1) != 42)
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
 ===================================================
  !! Expectation Failed: Second element is not 42
 ===================================================
```
##### Nested Collections
```csharp
    from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.ExpectWhen("List[0][0] is not 42",
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
 ===============================================
  !! Expectation Failed: List[0][0] is not 42
 ===============================================
```
##### Multiple Causes
This test is skipped because Trackr.StandIn is removed.  
```csharp
    // from substite in Trackr.StandIn(666)
    from input in Checkr.Input("Input", Fuzzr.Constant<IEnumerable<int>>([1, 2, 1]))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.ExpectWhen("input.Count() > 2, () => !input.Contains(1)",
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
##### Multiple Causes Default In Spec
This test is skipped because Trackr.StandIn is removed.  
```csharp
    // from substite in Trackr.StandIn(666)
    from input in Checkr.Input("Input", Fuzzr.Constant<IEnumerable<int>>([0, 2, 0]))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.ExpectWhen("input.Count() > 2, () => !input.Contains(0)",
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
##### Empty List Is The Culprit
```csharp
    from list in Checkr.Input("List", Fuzzr.Int().Many(0, 10))
    from listNotEmpty in Checkr.Expect("List is not empty", () => list.Any())
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\H_EmptyListIsTheCulprit.cs:19:1
 Original failing run:    1 execution
 Seed:                    2077876521
 ------------------------------------------------------------
  Executed:
   - Input: List = [  ]
 ============================================
  !! Expectation Failed: List is not empty
 ============================================
```
##### Array Shrinking
```csharp
var array = new[] { 42, 2, 3 };
var check =
    from input in Checkr.Input("int[]", Fuzzr.Constant(array))
    from spec in Checkr.ExpectWhen("int[0] is not 42",
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
#### Objects
##### Removes Irrelevant Properties
```csharp
public class BaggyInters
{
    public int IntOne { get; set; }
    public int IntTwo { get; set; }
    public int IntThree { get; set; }
}
```
```csharp
    from obj in Checkr.Input("Object", Fuzzr.One<BaggyInters>())
    from spec in Checkr.Expect("IntOne /= 42", () => obj.IntOne != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\A_RemovesIrrelevantProperties.cs:21:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    488581569
 ------------------------------------------------------------
  Executed:
   - Input: Object = { IntOne: 42 }
 =======================================
  !! Expectation Failed: IntOne /= 42
 =======================================
```
##### Nested Objects
```csharp
    from input in Checkr.Input("Universe",
        from _ in Configr.Primitive(Fuzzr.Constant("Answer me"))
        from __ in Configr.Primitive(Fuzzr.Int())
        from thing in Fuzzr.One<TheUniverse>()
        select thing)
    from spec in Checkr.ExpectWhen("It's not 42",
        () => input.Everything != null,
        () => input.Everything!.Answer != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\B_NestedObjects.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    803555194
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { Everything: { Answer: 42 } }
 ======================================
  !! Expectation Failed: It's not 42
 ======================================
```
##### Records
```csharp
public record Person(string Name, int Age);
```
```csharp
    from person in Checkr.Input("Person",
        from name in Fuzzr.String()
        from age in Fuzzr.Int()
        select new Person(name, age))
    from isNotUnderage in Checkr.Expect("Is not underage", () => person.Age >= 18)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\C_Records.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1556090131
 ------------------------------------------------------------
  Executed:
   - Input: Person = { Age: 4 }
 ==========================================
  !! Expectation Failed: Is not underage
 ==========================================
```
##### Irrelevant Objects
```csharp
    from person in Checkr.Input("Person", Fuzzr.One<Person>())
    from otherPerson in Checkr.Input("Other Person", Fuzzr.One<Person>())
    from tracePerson in Checkr.Trace("Person", () => person)
    from traceOtherTurtle in Checkr.Trace("Other Person", () => otherPerson)
    from isEighteen in Checkr.Expect("Not aged 18", () => person.Age != 18)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\D_IrrelevantObjects.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1000847613
 ------------------------------------------------------------
  Executed:
   - Input: Person = { Age: 18 }
   - Trace: Person = { Name: "voddj", Age: 18 }
   - Trace: Other Person = { Name: "hdpjy", Age: 85 }
 ======================================
  !! Expectation Failed: Not aged 18
 ======================================
```
##### Irrelevant Objects Null Issues
Can't shrink/remove 'Other Turtle' because of the null property, see top of ObjectShrinkStrategy.

Check this with: 
* `Investigations = [Investigate.NullIssues]`  
```csharp
    from turtle in Checkr.Input("Turtle",
        from depth in Configr<Turtle>.Depth(1, 3)
        from turtle in Fuzzr.One<Turtle>()
        select turtle)
    from otherTurtle in Checkr.Input("Other Turtle", Fuzzr.One<Turtle>())
    from traceTurtle in Checkr.Trace("Turtle", () => turtle)
    from traceOtherTurtle in Checkr.Trace("Other Turtle", () => otherTurtle)
    from endsOnTurtle in Checkr.ExpectWhen("Turtle Ends on 'TurtlesAllTheWay'",
        () => turtle.Down?.Down != null,
        () => turtle.Down?.Down?.GetType() == typeof(TurtlesAllTheWay))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\D1_IrrelevantObjectsNullIssues.cs:25:1
 Original failing run:    1 execution
 Seed:                    346547347
 ------------------------------------------------------------
  Executed:
   - Input: Turtle = { Down: { Down: { Down: null } } }
   - Input: Other Turtle = { Down: { Down: null } }
   - Trace: Turtle = { Down: { Down: { Down: null } } }
   - Trace: Other Turtle = { Down: { Down: null } }
   - WARNING: Null value for type 'Turtle' at input 'Turtle.Down.Down.Down'.
   - WARNING: Null value for type 'Turtle' at input 'Other Turtle.Down.Down'.
 ============================================================
  !! Expectation Failed: Turtle Ends on 'TurtlesAllTheWay'
 ============================================================
```
##### Checking Nulls
###### Catching a Null Reference Exception
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
In addition, QuickCheckr reports a shrinking warning:  
> Null value for type 'Life' at input 'The Universe.Everything'.  
This tells you that the shrinker reached a `null` value that it cannot safely reduce any further.  
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:28:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    358877731
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 78 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 168
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass2_1.GetCheckr() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 63
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 71
 ===========================================================================
```
But when both `Everything` *and* `Irrelevant` are `null`, QuickCheckr cannot shrink either property, and both limitations are reported as warnings:  
```text
------------------------------------------------------------
 Test:                    RunTest_NotShrinkingNull
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:74:1
 Original failing run:    1 execution
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null, Irrelevant: null }
   - Trace: The Universe = { Everything: null, Irrelevant: null }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
   - WARNING: Null value for type 'Life' at input 'The Universe.Irrelevant'.
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 168
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass2_1.GetCheckr() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 63
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 71
 ===========================================================================
```
The current workaround to avoid the irrelevant input being reported as relevant is to help the shrinker by rewriting your checkr.  
```csharp
    from everything in Checkr.Input("Everything", Fuzzr.One<Life>().NullableRef(), a => a with { FuzzrShrinker = 10 })
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.One<Life>().NullableRef(), a => a with { FuzzrShrinker = 10 })
    from universe in Checkr.Capture(() => new TheUniverse { Everything = everything, Irrelevant = irrelevant })
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Alternative
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:105:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: Everything = null
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 168
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass7_2.GetCheckr_Alternative() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 135
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 71
 ===========================================================================
```
Or, you can solve the issue using a `DeliberationPolicy` that limits the number of warnings:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumWarnings };
```
```text
------------------------------------------------------------
 Test:                    RunTest_WithDeliberationPolicy
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:149:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    90587887
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 86 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! Exception Thrown: System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.TheUniverse.GetTheAnswer() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 168
   at QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.E_CheckingNulls.<>c__DisplayClass2_1.GetCheckr() in Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:line 63
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 71
 ===========================================================================
```
As you can see the warning is still there for `Universe.Everything` but this is a relevant input,
so the warning can be ignored.  
Deliberation did succeed in removing the `Universe.Irrelevant` input from the report.  
##### Multiple Relevant Properties
This test is skipped because Trackr.StandIn is removed.  
```csharp
    // from standIn in Trackr.StandIn(123)
    from obj in Checkr.Input("BaggyInters", Fuzzr.One<BaggyInters>())
    from trace in Checkr.Trace("Original", () => obj)
    from spec in Checkr.Expect("obj.IntOne == 1 || obj.IntThree == 1",
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
##### Shrinking Records
```csharp
    from person in Checkr.Input("Person",
        from ctor in Configr<PersonRecord>.Construct(Fuzzr.String(), Fuzzr.Int())
        from personRecord in Fuzzr.One<PersonRecord>()
        select personRecord)
    from tracePerson in Checkr.Trace("Person", () => person)
    from isEighteen in Checkr.Expect("Not aged 18", () => person.Age != 18)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\G_ShrinkingRecords.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1121486451
 ------------------------------------------------------------
  Executed:
   - Input: Person = { Age: 18 }
   - Trace: Person = { Name: "xnh", Age: 18 }
 ======================================
  !! Expectation Failed: Not aged 18
 ======================================
```
#### Custom Shrinkers
```csharp
    from one in Checkr.Input("One", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
    from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
    from traceOne in Checkr.Trace("Original One", () => one)
    from traceTwo in Checkr.Trace("Original Two", () => two)
    from spec in Checkr.Expect("One should not be 42", () => one != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\T_CustomShrinkers\CustomShrinkers.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2122063655
 ------------------------------------------------------------
  Executed:
   - Input: One = 42
   - Trace: Original One = 42
   - Trace: Original Two = 90
 ===============================================
  !! Expectation Failed: One should not be 42
 ===============================================
```
#### Fuzzr Shrinking
```csharp
    from one in Checkr.Input("One", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from traceOne in Checkr.Trace("Original One", () => one)
    from traceTwo in Checkr.Trace("Original Two", () => two)
    from spec in Checkr.Expect("One should not be 42", () => one != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\T_CustomShrinkers\FuzzrShrinking.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2122063655
 ------------------------------------------------------------
  Executed:
   - Input: One = 42
   - Trace: Original One = 42
   - Trace: Original Two = 90
 ===============================================
  !! Expectation Failed: One should not be 42
 ===============================================
```
#### Input Reduction
```csharp
    from i in Checkr.Input("Int", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from spec in Checkr.Expect("Int should be smaller than 42", () => i < 42)
    from trace in Checkr.Trace("Int", () => i)
    select Case.Closed;
```
```csharp
private static IEnumerable<int> TowardsZero(int input)
{
    var local = input;
    while (local != 0)
    {
        if (local > 0)
            yield return --local;
        else
            yield return ++local;
    }
}
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\W_Reducers\InputReduction.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Int = 42
   - Trace: Int = 79
 ========================================================
  !! Expectation Failed: Int should be smaller than 42
 ========================================================
```
### Parsing Numbers
#### The Checkr:
```csharp
    from input in Checkr.Input("Expression", ParseFuzzr.Expression(), a => a with { Reducer = ParseReducer.Reducer })
    from trace in Checkr.Trace("Original", () => input)
    from goldenResult in Checkr.Act("Golden Result", () => LostIn.Translation(input).Eval())
    from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
    from traceResult in Checkr.Trace("Compare", () => $"Golden: {goldenResult} /= SUT: {result}")
    from match in Checkr.ExpectWhen("Match",
        () => !double.IsNaN(goldenResult),
        () => goldenResult == result)
    select Case.Closed;
```
#### The Configuration:
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with
    {
        DeliberationPolicy = (a, b) =>
            GetExpressionInputLength(a) < GetExpressionInputLength(b) ? a : b
    };
```
#### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\F_BruteForceShrinking\ParsingNumbers.cs:26:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1686948589
 ------------------------------------------------------------
  Executed: Golden Result, SUT Result
   - Input: Expression = "-1^2"
   - Trace: Original = - 1 ^ 2 / 1 * 3
   - Trace: Compare = Golden: -3 /= SUT: 3
 ================================
  !! Expectation Failed: Match
 ================================
```
## Building Blocks
### Checkr Act Carefully
```csharp
    from result in Checkr.ActCarefully("Act", () => throw new Exception("BOOM"))
    from throws in Checkr.Expect("Throws", () => result.Threw)
    from throwsExact in Checkr.Expect("Throws InvalidOperationException", result.ThrewAs<InvalidOperationException>)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\B_BuildingBlocks\H_CheckrActCarefully\CheckrActCarefully.cs:18:1
 Original failing run:    1 execution
 Seed:                    412717959
 ------------------------------------------------------------
  Executed: Act
 ===========================================================
  !! Expectation Failed: Throws InvalidOperationException
 ===========================================================
 Passed Expectations
 - Throws: 1x
 ------------------------------------------------------------
```
### Trackr Verify
```csharp
    from observer in Trackr.Stashed(() => new HashSet<int>())
    // using quickfuzzr below, but really it's a SUT
    from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
    from assay in Trackr.Verify("Contains 3", () => observer.Contains(3)) // <= gen = [1, 3) : fails
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\B_BuildingBlocks\S_TrackrVerify\TrackrVerify.cs:19:1
 Original failing run:    20 executions
 Seed:                    293407282
 ------------------------------------------------------------
  Executed: Roll (20 Times)
 ==========================================
  !!  The Verifier Disagrees: Contains 3
 ==========================================
```
```csharp
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
    from assay in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Multiple
 Location:                Notes\B_BuildingBlocks\S_TrackrVerify\TrackrVerify.cs:53:1
 Original failing run:    20 executions
 Seed:                    293407282
 ------------------------------------------------------------
  Executed: Roll (20 Times)
 =============================================================
  !!  The Verifier Disagrees: Rolls: Contains 3, Contains 4
 =============================================================
```
### Trackr Stop When Proven
```csharp
    from counter in Checkr.Act("hack for asserting execution count", () => ++executionCount)
    from spec in Trackr.StopWhenProven("Counter equals 3", () => counter == 3)
    from shouldNeverFail in Checkr.Expect("Should never fail, because of StopWhenProven", () => counter <= 3)
    select Case.Closed;
```
## Examples
### The Element Remover
This test focuses on a simple faulty implementation:  
```csharp
public static class ElementRemover
{
    public static List<int> Remove(List<int> list, int item)
    {
        list.Remove(item);
        return list;
    }
}
```
`ElementRemover` only removes the *first* occurrence of a value in a list.  
Using one input, one action, and one specification,
the test captures the minimal failing case where the list contains multiple copies of the target element.  
```csharp
    from list in Checkr.Input("List", GetFuzzr())
    from element in Checkr.Input("Item to Remove", Fuzzr.Int(0, 10))
    from output in Checkr.Act("Delete from List",
        () => ElementRemover.Remove([.. list], element))
    from listDoesNotContainElement in Checkr.Expect(
        "List Does Not Contain Element",
        () => !output.Contains(element))
    from traceList in Checkr.Trace("List Before Shrinking", () => list)
    select Case.Closed;
```
QuickCheckr reduces the failing run to a list like `[ 5, 5 ]` with `5` as the element to remove,
exposing the bug clearly: the output still contains `5`.  
The generated report shows the input, the attempted deletion, a trace of the original list before shrinking and a trace of the result from the action.  
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\A_TheElementRemover\TheElementRemover.cs:31:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    438724296
 ------------------------------------------------------------
  Executed: Delete from List
   - Input: List = [ 5, 5 ]
   - Input: Item to Remove = 5
   - Trace: List Before Shrinking = [ 2, 8, 0, 5, 5, 1, 8, 2, 9 ]
 ========================================================
  !! Expectation Failed: List Does Not Contain Element
 ========================================================
```
### Bugging The House
This is a set of intentionally fragile programs, each with its own peculiar way of failing.  
They range from simple state-based traps to intricate multi-method booby traps.  
#### Delayed Detonation
```csharp
public class BugHouse
{
    private int count;
    public bool Run(int a)
    {
        return !(count++ == 2 && a == 1);
    }
}
```
Throws only after exactly three total runs when the input value is 1.  
Demonstrates a simple stateful failure triggered by run count.  
```csharp
    from a in Checkr.Input("a", Fuzzr.Int())
    from bughouse in Trackr.Stashed(() => new BugHouse())
    from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
    from spec in Checkr.Expect("Returns true", () => output)
    select Case.Closed;
```
##### Minimal Fail Case:
- **Length:** 3 executions.
- **Example input:** `[42, 666, 1]`
```csharp
count = 0
Run( a = <any> ) // count = 1
Run( a = <any> ) // count = 2
Run( a = 1 ) // count == 2 && a == 1 → fail
```  
##### Report:
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\A_DelayedDetonation.cs:43:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1914853878
 ------------------------------------------------------------
  Executed: BugHouse.Run (2 Times)
 ------------------------------------------------------------
  Executed: BugHouse.Run
   - Input: a = 1
 =======================================
  !! Expectation Failed: Returns true
 =======================================
 Passed Expectations
 - Returns true: 2x
 ------------------------------------------------------------
```
#### Third Times the Harm
```csharp
public class BugHouse
{
    private int count;
    public bool Run(int a)
    {
        if (a == 6) count++;
        return !(count == 3);
    }
}
```
Throws as soon as the value 6 has been seen three times.  
A straightforward counter-based failure condition.  
```csharp
    from a in Checkr.Input("a", Fuzzr.Int())
    from bughouse in Trackr.Stashed(() => new BugHouse())
    from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
    from spec in Checkr.Expect("Returns true", () => output)
    select Case.Closed;
```
##### Minimal Fail Case:
- **Length:** 3 executions.
- **Example input:** `[6, 6, 6]`
```csharp
count = 0
Run( a = 6 ) // count = 1
Run( a = 6 ) // count = 2
Run( a = 6 ) // count == 3 → fail
```  
##### Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\B_ThirdTimesTheHarm.cs:34:1
 Original failing run:    36 executions
 Minimal failing case:    3 executions (after 33 shrinks)
 Seed:                    766917706
 ------------------------------------------------------------
  Executed: BugHouse.Run (3 Times)
   - Input: a = 6
 =======================================
  !! Expectation Failed: Returns true
 =======================================
 Passed Expectations
 - Returns true: 35x
 ------------------------------------------------------------
```
#### Convoluted Countdown
```csharp
public class BugHouse
{
	private int count;
	public bool Run(int a)
	{
		if (a == 6 && count != 3) count++;
		if (count >= 3) count++;
		if (count == 5) return false; return true;
	}
}
```
Fails after a more complex sequence: specific inputs increment the counter in different ways, 
requiring a particular input dance to trigger the failure.  
```csharp
from a in Checkr.Input("a", Fuzzr.Int())
from bughouse in Trackr.Stashed(() => new BugHouse())
from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
from spec in Checkr.Expect("Returns true", () => output)
select Case.Closed;
```
##### Minimal Fail Case:
- **Length:** 4 executions.
- **Example input:** `[6, 6, 6, 42]`
```csharp
count = 0
Run( a = 6 ) // +1 → 1
Run( a = 6 ) // +1 → 2
Run( a = 6 ) // +1 → 3, then >=3 → +1 → 4
Run( a != 6 ) // skip first inc, >=3 → +1 → 5 → fail
```  
##### Reports:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\B_ThirdTimesTheHarm.cs:34:1
 Original failing run:    36 executions
 Minimal failing case:    3 executions (after 33 shrinks)
 Seed:                    766917706
 ------------------------------------------------------------
  Executed: BugHouse.Run (3 Times)
   - Input: a = 6
 =======================================
  !! Expectation Failed: Returns true
 =======================================
 Passed Expectations
 - Returns true: 35x
 ------------------------------------------------------------
```
#### Multiple Methods
```csharp
public class BugHouse
{
	private string? bug;
	public bool RunInt(int a)
	{
		bug += "1";
		if (bug.EndsWith("1221") && a == 6) return false;
		return true;
	}
	public bool RunString(string a)
	{
		bug += "2";
		if (bug.EndsWith("122") && a == "p") return false;
		return true;
	}
}
```
Exception occurs only from a specific interleaving of RunInt and RunString calls, each appending different markers to state.
Demonstrates multi-method state interaction.  
```csharp
	from bughouse in Trackr.Stashed(() => new BugHouse())
	from operations in Checkr.OneOf(
		// --
		from i in Checkr.Input("Int", Fuzzr.Int())
		from output in Checkr.Act("BugHouse.RunInt", () => bughouse.RunInt(i))
		from spec in Checkr.Expect("RunInt s true", () => output)
		select Case.Closed,
		// --
		from str in Checkr.Input("String", Fuzzr.String())
		from output in Checkr.Act("BugHouse.RunString", () => bughouse.RunString(str))
		from spec in Checkr.Expect("RunString s true", () => output)
		select Case.Closed)
		//--
	select Case.Closed;
```
##### Minimal Fail Case (one example):
- **Length:** 4 executions.
- **Example input sequence:**
```csharp
RunInt(<any>)        // bug = "1"
RunString(<any>)     // bug = "12"
RunString(<any>)     // bug = "122"
RunInt(6)            // bug ends with "1221" and `a == 6` → fail
```  
##### Reports:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\D_MultipleMethods.cs:34:1
 Original failing run:    59 executions
 Minimal failing case:    4 executions (after 58 shrinks)
 Seed:                    1292206393
 ------------------------------------------------------------
  Executed: BugHouse.RunInt
 ------------------------------------------------------------
  Executed: BugHouse.RunString (2 Times)
 ------------------------------------------------------------
  Executed: BugHouse.RunInt
   - Input: Int = 6
 ==============================================
  !! Expectation Failed: RunInt returns true
 ==============================================
 Passed Expectations
 - RunString returns true: 34x
 - RunInt returns true: 24x
 ------------------------------------------------------------
```
### A Stateful List
**The Model:**  
```csharp
public class ListWrap
{
    private readonly List<int> list = [];
    public IReadOnlyList<int> List => list;
    public void Add(int item) => list.Add(item);
    public void Remove(int item) => list.Remove(item);
}
```
**The Checkr:**  
```csharp
    from listWrap in Trackr.Stashed(() => new ListWrap())
    from item in Checkr.Input("Item", Fuzzr.Int(0, 10))
    from traceBefore in Checkr.Trace("Before", () => listWrap.List)
    from operations in Checkr.OneOf(
        from add in Checkr.Act("Add", () => listWrap.Add(item))
        from listContainsItem in Checkr.Expect("List Contains Item",
            () => listWrap.List.Contains(item))
        select Case.Closed,
        from remove in Checkr.Act("Remove", () => listWrap.Remove(item))
        from listDoesNotContainItem in Checkr.Expect("List Does Not Contain Item",
            () => !listWrap.List.Contains(item))
        select Case.Closed)
    from traceAfter in Checkr.Trace("After", () => listWrap.List)
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Notes\T_Examples\C_AStatefulList\AStatefulList.cs:32:1
 Original failing run:    9 executions
 Minimal failing case:    3 executions (after 6 shrinks)
 Seed:                    254875886
 ------------------------------------------------------------
  Executed: Add
   - Input: Item = 3
   - Trace: Before = [ ]
   - Trace: After = [ 3 ]
 ------------------------------------------------------------
  Executed: Add
   - Input: Item = 3
   - Trace: Before = [ 3 ]
   - Trace: After = [ 3, 3 ]
 ------------------------------------------------------------
  Executed: Remove
   - Input: Item = 3
   - Trace: Before = [ 3, 3 ]
   - Trace: After = [ 3 ]
 =====================================================
  !! Expectation Failed: List Does Not Contain Item
 =====================================================
 Passed Expectations
 - List Does Not Contain Item: 4x
 - List Contains Item: 4x
 ------------------------------------------------------------
```
### Parsing Numbers
```csharp
    from input in Checkr.Input("Expression", ParseFuzzr.Expression(), a => a with { Reducer = ParseReducer.Reducer })
    from trace in Checkr.Trace("Original", () => input)
    from goldenResult in Checkr.Act("Golden Result", () => LostIn.Translation(input).Eval())
    from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
    from traceResult in Checkr.Trace("Compare", () => $"Golden: {goldenResult} /= SUT: {result}")
    from match in Checkr.ExpectWhen("Match",
        () => !double.IsNaN(goldenResult),
        () => goldenResult == result)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\Q_ParsingNumbers\ParsingNumbers.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    406406480
 ------------------------------------------------------------
  Executed: Golden Result, SUT Result
   - Input: Expression = "2^-2^2"
   - Trace: Original = (2 * 1 - 3 * 2 * 1 * 3 + 1 / 2 * 1) ^ - 2 ^ 2 / 3 ^ 2 / 3 - - 3 / 2
   - Trace: Compare = Golden: 1,5000006416666136 /= SUT: 2139,2800925925926
 ================================
  !! Expectation Failed: Match
 ================================
```
### Scheduling
#### The Model:
```csharp
public class Booking
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public Schedule Schedule { get; set; } = new Schedule();

    public bool OverlapsWith(Booking otherBooking)
    {
        var start = Max(Start, otherBooking.Start);
        var end = Min(End, otherBooking.End);
        if (end < start) return false;
        if (SlotsOverlap(Schedule.Monday, otherBooking.Schedule.Monday)) return true;
        if (SlotsOverlap(Schedule.Tuesday, otherBooking.Schedule.Tuesday)) return true;
        if (SlotsOverlap(Schedule.Wednesday, otherBooking.Schedule.Wednesday)) return true;
        if (SlotsOverlap(Schedule.Thursday, otherBooking.Schedule.Thursday)) return true;
        if (SlotsOverlap(Schedule.Friday, otherBooking.Schedule.Friday)) return true;
        return false;
    }

    private static bool SlotsOverlap(List<Timeslot> slotsOne, List<Timeslot> slotsTwo)
    {
        foreach (var slotOne in slotsOne)
            foreach (var slotTwo in slotsTwo)
                if (slotOne.OverlapsWith(slotTwo))
                    return true;
        return false;
    }

    private static DateOnly Max(DateOnly x, DateOnly y) => x > y ? x : y;
    private static DateOnly Min(DateOnly x, DateOnly y) => x < y ? x : y;
}

public class Schedule
{
    public List<Timeslot> Monday { get; set; } = [];
    public List<Timeslot> Tuesday { get; set; } = [];
    public List<Timeslot> Wednesday { get; set; } = [];
    public List<Timeslot> Thursday { get; set; } = [];
    public List<Timeslot> Friday { get; set; } = [];
}

public class Timeslot
{
    public int Start { get; set; }
    public int End { get; set; }
    public bool OverlapsWith(Timeslot otherTimeSlot)
    {
        if (Start < otherTimeSlot.End && End > otherTimeSlot.Start) return true;
        return false;
    }
}
```
#### The Fuzzr:
```csharp
namespace QuickCheckr.Tests.Notes.T_Examples.V_Scheduling;

public class TheFuzzr
{
    private readonly static FuzzrOf<Timeslot> ValidTimeSlot =
        from start in Fuzzr.Int(10, 13)
        from end in Fuzzr.Int(13, 16)
        select new Timeslot { Start = start, End = end };

    public readonly static FuzzrOf<Booking> ValidBooking =
        from start in Fuzzr.DateOnly(1.December(2025), 30.December(2025))
        from monday in ValidTimeSlot
        from tuesday in ValidTimeSlot
        from wednesday in ValidTimeSlot
        from thursday in ValidTimeSlot
        from friday in ValidTimeSlot
        select new Booking
        {
            Start = start,
            End = start.AddDays(6),
            Schedule = new Schedule
            {
                Monday = [monday],
                Tuesday = [tuesday],
                Wednesday = [wednesday],
                Thursday = [thursday],
                Friday = [friday]
            }
        };

    public static FuzzrOf<Booking> NonOverlappingBooking(Booking booking)
    {
        return
            from start in Fuzzr.DateOnly(1.December(2025), 30.December(2025))
            from monday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Monday)
            from tuesday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Tuesday)
            from wednesday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Wednesday)
            from thursday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Thursday)
            from friday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Friday)
            select new Booking
            {
                Start = start,
                End = start.AddDays(6),
                Schedule = new Schedule
                {
                    Monday = [monday],
                    Tuesday = [tuesday],
                    Wednesday = [wednesday],
                    Thursday = [thursday],
                    Friday = [friday]
                }
            };
    }

    private static DateOnly WeekdayInThis7DayRange(DateOnly start, DayOfWeek weekDay) =>
        start.AddDays(((int)weekDay - (int)start.DayOfWeek + 7) % 7);

    private static FuzzrOf<Timeslot> NonOverlappingTimeSlot(DateOnly startDate, Booking booking, DayOfWeek weekDay)
    {
        var day = WeekdayInThis7DayRange(startDate, weekDay);
        var otherBookingDay = WeekdayInThis7DayRange(booking.Start, weekDay);
        if (day != otherBookingDay)
            return
                from start in Fuzzr.Int(8, 17)
                from end in Fuzzr.Int(start + 1, 18)
                select new Timeslot { Start = start, End = end };
        var otherSlot = GetSlotForDay(booking, weekDay);
        return
            from start1 in Fuzzr.Int(8, otherSlot.Start - 1)
            from end1 in Fuzzr.Int(start1 + 1, otherSlot.Start)
            from before in Fuzzr.One(() => new Timeslot { Start = start1, End = end1 })
            from start2 in Fuzzr.Int(otherSlot.End, 17)
            from end2 in Fuzzr.Int(start2 + 1, 18)
            from after in Fuzzr.One(() => new Timeslot { Start = start1, End = end2 })
            from slot in Fuzzr.OneOf(before, after)
            select slot;
    }

    private static Timeslot GetSlotForDay(Booking booking, DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => booking.Schedule.Monday.First(),
            DayOfWeek.Tuesday => booking.Schedule.Tuesday.First(),
            DayOfWeek.Wednesday => booking.Schedule.Wednesday.First(),
            DayOfWeek.Thursday => booking.Schedule.Thursday.First(),
            DayOfWeek.Friday => booking.Schedule.Friday.First(),
            _ => throw new ArgumentOutOfRangeException(nameof(day), day, null)
        };
    }
}
```
#### The Checkr:
```csharp
    from bookingOne in Checkr.Input("Booking One", TheFuzzr.ValidBooking)
    from bookingTwo in Checkr.Input("Booking Two", TheFuzzr.NonOverlappingBooking(bookingOne))
    from Spec in Checkr.Expect("Bookings do not overlap", () => !bookingOne.OverlapsWith(bookingTwo))
    select Case.Closed;
```
#### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\V_Scheduling\Scheduling.cs:25:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    511619818
 ------------------------------------------------------------
  Executed:
   - Input: Booking One = { Start: 27.December(2025), End: 2.January(2026), Schedule: { Thursday: [ { Start: 10, End: 15 } ] } }
   - Input: Booking Two = { Start: 25.December(2025), End: 31.December(2025), Schedule: { Thursday: [ { Start: 14, End: 16 } ] } }
 ==================================================
  !! Expectation Failed: Bookings do not overlap
 ==================================================
```
## It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
### Input
- `Checkr.Input`:  
```csharp
    from one in Checkr.Input("reused", Fuzzr.Int())
    from two in Checkr.Input("reused", Fuzzr.Int())
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Input' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Input' uses a unique key.
```
### Act Carefully
- `Checkr.ActCarefully`:  
```csharp
    from one in Checkr.ActCarefully("reused", () => { })
    from two in Checkr.ActCarefully("reused", () => { })
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Act' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Act' uses a unique key.
```
### Spec
- `Checkr.Spec`:  
```csharp
    from one in Checkr.Expect("reused", () => true)
    from two in Checkr.Expect("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Expect' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Expect' uses a unique key.
```
### Trace
- `Checkr.Trace`:  
```csharp
    from one in Checkr.Trace("reused", () => "")
    from two in Checkr.Trace("reused", () => "")
    from spec in Checkr.Expect("fails", () => false) // <= this one needs to run
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Trace' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Trace' uses a unique key.
```
### One Of
- `Checkr.OneOf`:  
```csharp
    from one in Checkr.OneOf(Checkr.Act("One", () => { }))
    from two in Checkr.OneOf(Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'OneOf|Default|QuickCheckr|Key' is written more than once in a single 'Checkr.OneOf' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.OneOf' uses a unique key.
```
### Stashed
- `Trackr.Stashed`:  
```csharp
    from one in Trackr.Stashed(() => 42)
    from two in Trackr.Stashed(() => 666)
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once in a single 'Trackr.Stashed' run.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Trackr.Stashed' uses a unique key.
```
### Pool
- `Trackr.PoolFor`:  
```csharp
    from one in Trackr.PoolFor<int>()
    from two in Trackr.PoolFor<int>()
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once in a single 'Trackr.PoolFor' run.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Trackr.PoolFor' uses a unique key.
```
### Verify
- `Trackr.Verify`:  
```csharp
    from one in Trackr.Verify("reused", () => true)
    from two in Trackr.Verify("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Expect' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Expect' uses a unique key.
```
### Stop When Proven
- `Trackr.StopWhenProven`:  
```csharp
    from one in Trackr.StopWhenProven("reused", () => false)
    from two in Trackr.StopWhenProven("reused", () => false)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Expect' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Expect' uses a unique key.
```
## Acting Out Of Sync
```csharp
public class Eventually
{
    public static Task<int> AddOne(int input)
    {
        return Task.FromResult(input + 1);
    }
}
```
```csharp
    from input in Checkr.Input("Input", Fuzzr.Int())
    from result in Checkr.Act("Eventually.AddOne", Eventually.AddOne(input))
    from spec in Checkr.Expect("Is not 42", () => result != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\Y_Async\ActingOutOfSync.cs:20:1
 Original failing run:    1 execution
 Seed:                    367926403
 ------------------------------------------------------------
  Executed: Eventually.AddOne
   - Input: Input = 41
 ====================================
  !! Expectation Failed: Is not 42
 ====================================
```
### Lock The Stable Doors
**The Checkr:**  
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from service in Trackr.Stashed(() => new InMemoryCoachCourseService())
    from coaches in Trackr.PoolFor<CoachBy>()
    from courses in Trackr.PoolFor<CourseBy>()
    from dbCourses in Checkr.Capture(service.GetCoursesAsync())
    from operations in Checkr.OneOfIf(
        (() => coaches.Count() < 10, CreateCoach),
        (() => coaches.Any(), UpdateCoachSkills),
        (() => courses.Count() < 5, CreateCourse),
        (() => courses.Any(), UpdateCourseSkills),
        (() => coaches.Any() && dbCourses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
    from invariant in AssignedCoachesMustBeSuitable
    select Case.Closed;
```
**Create Coach Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from trace in Checkr.Trace("Coach Name", () => name)
    from coach in Checkr.Act("Create Coach", service.CreateCoachAsync(name))
    from toPool in Trackr.Pool("Store Coach", new CoachBy(name))
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCoachSkills =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from coach in Trackr.OneOfPool<CoachBy>("Coach to update")
    from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from valid in Checkr.When(() => coach is not null,
        from trace in Checkr.Trace("Coach Name", () => coach.Name)
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => service.UpdateCoachSkillsAsync(coach.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Create Course Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCourse =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Course Name",
        from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
    from trace in Checkr.Trace("Course Name", () => name)
    from course in Checkr.Act("Create Course", service.CreateCourseAsync(name))
    from toPool in Trackr.Pool("Store Course", new CourseBy(name))
    select Case.Closed;
```
**Update Course Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCourseSkills =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from course in Trackr.OneOfPool<CourseBy>("Course to update")
    from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from courseNotNull in Checkr.When(() => course is not null,
        from traceCourse in Checkr.Trace("Course Name", () => course.Name)
        from updateSkills in Checkr.Act("Update Course Skills",
            service.UpdateCourseSkillsAsync(course.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from coaches in Checkr.Capture(service.GetCoachesAsync())
    from assignedCheck in Checkr.ExpectWhen("Assigned Coaches Must Be Suitable",
        () => coaches.Any(coach => coach.AssignedCourses.Any()),
        () => coaches.All(
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course))))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\Y_Async\B_LockTheStableDoors\LockTheStableDoors.cs:32:1
 Original failing run:    11 executions
 Minimal failing case:    4 executions (after 12 shrinks)
 Seed:                    1511313504
 ------------------------------------------------------------
  Executed: Create Coach
   - Trace: Coach Name = Coach-1
 ------------------------------------------------------------
  Executed: Create Course
   - Trace: Course Name = Course-2
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - Trace: Coach Name = Coach-1
   - Trace: Course Name = Course-2
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Input: Course Skills = [ _ ]
   - Trace: Course Name = Course-2
 ============================================================
  !! Expectation Failed: Assigned Coaches Must Be Suitable
 ============================================================
 Passed Expectations
 - Assigned Coaches Must Be Suitable: 5x
 ------------------------------------------------------------
```
