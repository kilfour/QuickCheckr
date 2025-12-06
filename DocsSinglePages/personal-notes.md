# Personal Notes
A temporary collection of documented tests to be moved into the documentation later.  
## Shrinking
Whenever QuickCheckr comes accross a failure during a run, 
it tries to shrink the failing case to find a minimal failing case.

During this process no further executions are performed and no new random values are generated.

QuickCheckr then reports the minimal failing case, 
together with the number of executions, relevant actions and inputs that were required to find it.  
### Execution Shrinking
This is the first step in QuickCheckr shrinking.  
The following simple example fails on the third execution.  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from spec in Checkr.Spec("Counter does not equal 3", () => counter != 3)
    select Case.Closed;
```

Execution 1: counter = 1  
Execution 2: counter = 2  
Execution 3: counter = 3  (Fails)  

During execution shrinking the values from counter are not regenerated, but retrieved from the previous run.
So the ExecutionShrinker retries the run with the same values, but tries to remove executions, one by one.  
If the run still fails, the execution is not deemed relevant for the failure and is removed.  

So after *Execution Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    ExampleTest
 Location:                Notes\A_Shrinking\A_ExecutionShrinking\ExecutionShrinking.cs:53:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ============================================
  !! Spec Failed: Counter does not equal 3
 ============================================
 Passed Specs
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
### Input Shrinking
This is the third step in QuickCheckr shrinking and the most complex one.  
QuickCheckr shrinking tries to remove inputs, one by one, to find a minimal failing case.  
The following simple example has one input but always fails.  
```csharp
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Spec("Always Fails", () => false)
    select Case.Closed;
```
Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after *Input Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\InputShrinking.cs:27:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    165641598
 ------------------------------------------------------------
  Executed
 ================================
  !! Spec Failed: Always Fails
 ================================
```
Another example, slightly trickier.  
```csharp
    from one in Checkr.Input("Input One", Fuzzr.Int())
    from two in Checkr.Input("Input Two", Fuzzr.Int())
    from three in Checkr.Input("Input Three", Fuzzr.Int())
    from result in Checkr.Act("Result", () => one + three)
    from spec in Checkr.Spec("Should not be 42", () => result != 42)
    select Case.Closed;
```
As you can see from the test definition, two inputs are involved in a possible failure.

QuickCheckr correctly identifies the relevant inputs.

Result:
  
```text
------------------------------------------------------------
 Test:                    Example_Two
 Location:                Notes\A_Shrinking\C_InputShrinking\InputShrinking.cs:65:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    231122398
 ------------------------------------------------------------
  Executed: Result
   - Input: Input One = 12
   - Input: Input Three = 30
 ====================================
  !! Spec Failed: Should not be 42
 ====================================
```
#### Enums
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<DayOfWeek>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<DayOfWeek>())
    from spec in Checkr.Spec("Should not be Monday", () => inputTwo != DayOfWeek.Monday)
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
 ========================================
  !! Spec Failed: Should not be Monday
 ========================================
```
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<SoundCheck>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<SoundCheck>())
    from spec in Checkr.Spec("Should not be Two", () => inputTwo != SoundCheck.Two)
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
 =====================================
  !! Spec Failed: Should not be Two
 =====================================
```
#### Collections
##### Removing Irrelevant Items
When shrinking collections, QuickCheckr does not attempt to rewrite or regenerate the list.
Instead, it removes items one by one, replaying the run each time to test whether the removed element was relevant for the failure.

This means that if only some elements in a list contribute to the failing condition, QuickCheckr will aggressively trim the rest away. The result is a minimal list containing only the values that still trigger the failure, even if the original list was large or noisy.

Consider a test that forbids the value `7`:  
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
    from spec in Checkr.Spec("List does not contain 7", () => !listInput.Contains(7))
    from trace in Checkr.Trace("List Input", () => listInput)
    select Case.Closed;
```
Even if the original list was long and varied, QuickCheckr isolates the single relevant element:  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\A_RemovingIrrelevantItems.cs:26:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1833095082
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 7 ]
   - Trace: List Input = [ 69, 49, 90, 36, 16, 71, 95, 7, 84 ]
 ===========================================
  !! Spec Failed: List does not contain 7
 ===========================================
```
##### When Relevant Item Is Default
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
##### Irrelevant Element But List Length Is Not
When shrinking collections, QuickCheckr isolates the structural aspects that contributed to the failure
and leaves irrelevant details untouched. 

In cases like this example where the predicate depends solely on the length of the list,
the individual element values play no role.  
QuickCheckr therefore preserves them as anonymous placeholders (_) and concentrates entirely on reducing the collection's shape:
removing elements, and converging toward the smallest list that still violates the condition.

This yields minimal, focused shrinking artifacts:
the report shows only the structural signal that mattered (here: the list being *too long*),
without introducing noise from element-level changes that could not affect the outcome anyway.  
```csharp
    from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
    from trace in Checkr.Trace("Original List", () => list)
    from spec in Checkr.Spec("Length <= 2", () => list.Count() <= 2)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\C_IrrelevantElementButListLengthIsNot.cs:30:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1393576429
 ------------------------------------------------------------
  Executed:
   - Input: List = [ _, _, _ ]
   - Trace: Original List = [ 12, 45, 56, 18, 31 ]
 ===============================
  !! Spec Failed: Length <= 2
 ===============================
```
##### Relevant Multiple Eelements
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
##### Relevant Element And Size
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
##### Nested Collections
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
##### Multiple Causes
```csharp
    from substite in Trackr.StandIn(666)
    from input in Checkr.Input("Input", Fuzzr.Constant<IEnumerable<int>>([1, 2, 1]))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.SpecIf("input.Count() > 2, () => !input.Contains(1)",
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
##### Array Shrinking
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
    from spec in Checkr.Spec("IntOne /= 42", () => obj.IntOne != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\A_RemovesIrrelevantProperties.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    488581569
 ------------------------------------------------------------
  Executed:
   - Input: Object = { IntOne: 42 }
 ================================
  !! Spec Failed: IntOne /= 42
 ================================
```
##### Nested Objects
```csharp
    from input in Checkr.Input("Universe",
        from _ in Configr.Primitive(Fuzzr.Constant("Answer me"))
        from __ in Configr.Primitive(Fuzzr.Int())
        from thing in Fuzzr.One<TheUniverse>()
        select thing)
    from spec in Checkr.SpecIf("It's not 42",
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
 ===============================
  !! Spec Failed: It's not 42
 ===============================
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
    from isNotUnderage in Checkr.Spec("Is not underage", () => person.Age >= 18)
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
 ===================================
  !! Spec Failed: Is not underage
 ===================================
```
##### Irrelevant Objects
```csharp
    from person in Checkr.Input("Person", Fuzzr.One<Person>())
    from otherPerson in Checkr.Input("Other Person", Fuzzr.One<Person>())
    from tracePerson in Checkr.Trace("Person", () => person)
    from traceOtherTurtle in Checkr.Trace("Other Person", () => otherPerson)
    from isEighteen in Checkr.Spec("Not aged 18", () => person.Age != 18)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\D_IrrelevantObjects.cs:22:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1000847613
 ------------------------------------------------------------
  Executed:
   - Input: Person = { Age: 18 }
   - Trace: Person = { Name: "voddj", Age: 18 }
   - Trace: Other Person = { Name: "hdpjy", Age: 85 }
 ===============================
  !! Spec Failed: Not aged 18
 ===============================
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
    from endsOnTurtle in Checkr.SpecIf("Turtle Ends on 'TurtlesAllTheWay'",
        () => turtle.Down?.Down != null,
        () => turtle.Down?.Down?.GetType() == typeof(TurtlesAllTheWay))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\D1_IrrelevantObjectsNullIssues.cs:21:1
 Original failing run:    1 execution
 Seed:                    346547347
 ------------------------------------------------------------
  Executed:
   - Input: Turtle = { Down: { Down: { Down: null } } }
   - Input: Other Turtle = { Down: { Down: null } }
   - Trace: Turtle = { Down: { Down: { Down: null } } }
   - Trace: Other Turtle = { Down: { Down: null } }
 =====================================================
  !! Spec Failed: Turtle Ends on 'TurtlesAllTheWay'
 =====================================================
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
##### Multiple Relevant Properties
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
##### Shrinking Records
```csharp
    from person in Checkr.Input("Person",
        from ctor in Configr<PersonRecord>.Construct(Fuzzr.String(), Fuzzr.Int())
        from personRecord in Fuzzr.One<PersonRecord>()
        select personRecord)
    from tracePerson in Checkr.Trace("Person", () => person)
    from isEighteen in Checkr.Spec("Not aged 18", () => person.Age != 18)
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
 ===============================
  !! Spec Failed: Not aged 18
 ===============================
```
#### Custom Shrinkers
```csharp
    from one in Checkr.Input("One", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
    from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
    from traceOne in Checkr.Trace("Original One", () => one)
    from traceTwo in Checkr.Trace("Original Two", () => two)
    from spec in Checkr.Spec("One should not be 42", () => one != 42)
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
 ========================================
  !! Spec Failed: One should not be 42
 ========================================
```
#### Fuzzr Shrinking
```csharp
    from one in Checkr.Input("One", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from traceOne in Checkr.Trace("Original One", () => one)
    from traceTwo in Checkr.Trace("Original Two", () => two)
    from spec in Checkr.Spec("One should not be 42", () => one != 42)
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
 ========================================
  !! Spec Failed: One should not be 42
 ========================================
```
#### Input Reduction
```csharp
    from i in Checkr.Input("Int", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from spec in Checkr.Spec("Int should be smaller than 42", () => i < 42)
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
 =================================================
  !! Spec Failed: Int should be smaller than 42
 =================================================
```
## Building Blocks
### Checkr Act Carefully
```csharp
    from result in Checkr.ActCarefully("Act", () => throw new Exception("BOOM"))
    from throws in Checkr.Spec("Throws", () => result.Threw)
    from throwsExact in Checkr.Spec("Throws InvalidOperationException", result.ThrewAs<InvalidOperationException>)
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
 ====================================================
  !! Spec Failed: Throws InvalidOperationException
 ====================================================
 Passed Specs
 - Throws: 1x
 ------------------------------------------------------------
```
### Trackr Assay
```csharp
    from observer in Trackr.Stashed(() => new HashSet<int>())
        // using quickfuzzr below, but really it's a SUT
    from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
    from assay in Trackr.Assay("Contains 3", () => observer.Contains(3)) // <= gen = [1, 3) : fails
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\B_BuildingBlocks\S_TrackrAssay\TrackrAssay.cs:19:1
 Original failing run:    20 executions
 Seed:                    293407282
 ------------------------------------------------------------
  Executed: Roll (20 Times)
 =========================================
  !!  The Assayer Disagrees: Contains 3
 =========================================
```
```csharp
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
    from assay in Trackr.Assay("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Multiple
 Location:                Notes\B_BuildingBlocks\S_TrackrAssay\TrackrAssay.cs:53:1
 Original failing run:    20 executions
 Seed:                    293407282
 ------------------------------------------------------------
  Executed: Roll (20 Times)
 ============================================================
  !!  The Assayer Disagrees: Rolls: Contains 3, Contains 4
 ============================================================
```
### Trackr Proven When
```csharp
    from counter in Checkr.Act("hack for asserting execution count", () => ++executionCount)
    from spec in Trackr.ProvenWhen("Counter equals 3", () => counter == 3)
    from shouldNeverFail in Checkr.Spec("Should never fail, because of ProvenWhen", () => counter <= 3)
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
    from listDoesNotContainElement in Checkr.Spec(
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
 =================================================
  !! Spec Failed: List Does Not Contain Element
 =================================================
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
    from spec in Checkr.Spec("Returns true", () => output)
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
 ================================
  !! Spec Failed: Returns true
 ================================
 Passed Specs
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
    from spec in Checkr.Spec("Returns true", () => output)
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
 ================================
  !! Spec Failed: Returns true
 ================================
 Passed Specs
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
from spec in Checkr.Spec("Returns true", () => output)
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
 ================================
  !! Spec Failed: Returns true
 ================================
 Passed Specs
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
	from ops in Checkr.OneOf(
		// --
		from i in Checkr.Input("Int", Fuzzr.Int())
		from output in Checkr.Act("BugHouse.RunInt", () => bughouse.RunInt(i))
		from spec in Checkr.Spec("RunInt s true", () => output)
		select Case.Closed,
		// --
		from str in Checkr.Input("String", Fuzzr.String())
		from output in Checkr.Act("BugHouse.RunString", () => bughouse.RunString(str))
		from spec in Checkr.Spec("RunString s true", () => output)
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
 =======================================
  !! Spec Failed: RunInt returns true
 =======================================
 Passed Specs
 - RunString returns true: 34x
 - RunInt returns true: 24x
 ------------------------------------------------------------
```
### There Is Always An Account
#### The Model:
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```
#### The Checkr:
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from trace in Checkr.Trace("Amount", () => amount)
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
#### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:25:1
 Original failing run:    5 executions
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 1
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 4x
 ------------------------------------------------------------
```
#### Original Run:
```text
------------------------------------------------------------
 Test:                    FullRunTest
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:50:1
 Original failing run:    5 executions
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed: Deposit
   - Trace: Amount = 94
 ------------------------------------------------------------
  Executed: Deposit
   - Trace: Amount = 59
 ------------------------------------------------------------
  Executed: Withdraw
   - Trace: Amount = 43
 ------------------------------------------------------------
  Executed: Withdraw
   - Trace: Amount = 63
 ------------------------------------------------------------
  Executed: Withdraw
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 4x
 ------------------------------------------------------------
```
### Parsing Numbers
```csharp
    from input in Checkr.Input("Expression", ParseFuzzr.Expression(), a => a with { Reducer = ParseReducer.Reducer })
    from trace in Checkr.Trace("Original", () => input)
    from goldenResult in Checkr.Act("Golden Result", () => LostIn.Translation(input).Eval())
    from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
    from traceResult in Checkr.Trace("Compare", () => $"Golden: {goldenResult} /= SUT: {result}")
    from match in Checkr.SpecIf("Match",
        () => !double.IsNaN(goldenResult),
        () => goldenResult == result)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\Q_ParsingNumbers\ParsingNumbers.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    406406480
 ------------------------------------------------------------
  Executed (1): Golden Result, SUT Result
   - Input: Expression = "2^-2^2"
   - Trace: Original = (2 * 1 - 3 * 2 * 1 * 3 + 1 / 2 * 1) ^ - 2 ^ 2 / 3 ^ 2 / 3 - - 3 / 2
   - Trace: Compare = Golden: 1,5000006416666136 /= SUT: 2139,2800925925926
 =========================
  !! Spec Failed: Match
 =========================
```
## It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail cases, and summaries.
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
    from one in Checkr.Spec("reused", () => true)
    from two in Checkr.Spec("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Spec' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Spec' uses a unique key.
```
### Trace
- `Checkr.Trace`:  
```csharp
    from one in Checkr.Trace("reused", () => "")
    from two in Checkr.Trace("reused", () => "")
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
### Assay
- `Trackr.Assay`:  
```csharp
    from one in Trackr.Assay("reused", () => true)
    from two in Trackr.Assay("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Spec' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Spec' uses a unique key.
```
### Proven When
- `Trackr.ProvenWhen`:  
```csharp
    from one in Trackr.ProvenWhen("reused", () => false)
    from two in Trackr.ProvenWhen("reused", () => false)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Spec' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Spec' uses a unique key.
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
    from spec in Checkr.Spec("Is not 42", () => result != 42)
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
 =============================
  !! Spec Failed: Is not 42
 =============================
```
### Lock The Stable Doors
**The Checkr:**  
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from service in Trackr.Stashed(() => new InMemoryCoachCourseService())
    from coaches in Trackr.PoolFor<CoachBy>()
    from courses in Trackr.PoolFor<CourseBy>()
    from dbCourses in Trackr.Capture(service.GetCoursesAsync())
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
    from coaches in Trackr.Capture(service.GetCoachesAsync())
    from assignedCheck in Checkr.SpecIf("Assigned Coaches Must Be Suitable",
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
 =====================================================
  !! Spec Failed: Assigned Coaches Must Be Suitable
 =====================================================
 Passed Specs
 - Assigned Coaches Must Be Suitable: 5x
 ------------------------------------------------------------
```
