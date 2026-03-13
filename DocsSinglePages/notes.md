# Notes
A temporary collection of documented tests to be moved into the documentation later.  
## Shrinking
Whenever QuickCheckr comes accross a failure during a run, 
it tries to shrink the failing case to find a minimal failing case.

During this process no further executions are performed and no new random values are generated.

QuickCheckr then reports the minimal failing case, 
together with the number of executions, relevant actions and inputs that were required to find it.  
### Issue Maybe

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(1),
    Shrink.OnType(
        [Try.Values(42)], // <= 42 is the required passing candidate
        [Reduce.Towards(100)]))
from trace in Checkr.Trace("answers", () => answers)
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                IssueMaybe.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    346495699
 ------------------------------------------------------------
  Executed:
   - Input: answers = [ 15 ]
            Redux     [ 41 ]
   - Trace: answers = [ 15 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```
##### Nullable Types

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Constant<Nullable<int>>(null!))
from expect in Checkr.Expect("Fails", () => input.HasValue)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NullableTypes.cs:64:1
 Original failing run:    1 execution
 Seed:                    393010076
 ------------------------------------------------------------
  Executed:
   - Input: Input = null
 ================================
  !! Expectation Failed: Fails
 ================================
```
#### Relevant Item Is Default
Just making sure `default` values, `0` in this case, get handled correctly.  

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(0, 10))
from expect in Checkr.Expect("List does not contain 0", () => !listInput.Contains(0))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_RelevantItemIsDefault.cs:59:1
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
#### Relevant Item Is All But Default
Just making sure `default` values, `0` in this case, get handled correctly.  

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(2, 10))
from expect in Checkr.Expect("List contains only 0", () => listInput.All(a => a == 0))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                E_RelevantItemIsAllButDefault.cs:59:1
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
##### Arrays
###### An Array As Input
Seeing as all inputs are provided by QuickFuzzr,
and the default generated enumerable underlying type is a list,
we have to cheat a bit using a `Fuzzr.Constant()` in order to pass in an Array.  

**The Checkr:**  
```csharp
from input in Checkr.Input("int[]", Fuzzr.Constant(new int[] { 42, 2, 3 }))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("No answer here",
    () => !input.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_AnArrayAsInput.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    27506614
 ------------------------------------------------------------
  Executed:
   - Input: int[]    = [ 42 ]
   - Trace: Original = [ 42, 2, 3 ]
 =========================================
  !! Expectation Failed: No answer here
 =========================================
```
##### List Containing Null Values

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int().Nullable().Many(0, 10))
from expect in Checkr.Expect("no nulls allowed", () => listInput.All(a => a.HasValue))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ListContainingNullValues.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1833095082
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ null ]
   - Trace: List Input = [ 49, 36, null, 95, null, 82, 73, 58, 2 ]
 ===========================================
  !! Expectation Failed: no nulls allowed
 ===========================================
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
from expect in Checkr.Expect("IntOne /= 42", () => obj.IntOne != 42)
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
from expect in Checkr.ExpectWhen("It's not 42",
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

**The Checkr:**  
```csharp
from person in Checkr.Input("Person", Fuzzr.One<Person>())
from otherPerson in Checkr.Input("Other Person", Fuzzr.One<Person>())
from tracePerson in Checkr.Trace("Person", () => person)
from traceOtherTurtle in Checkr.Trace("Other Person", () => otherPerson)
from isEighteen in Checkr.Expect("Not aged 18", () => person.Age != 18)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_IrrelevantObjects.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1000847613
 ------------------------------------------------------------
  Executed:
   - Input: Person       = { Age: 18 }
   - Trace: Person       = { Name: "voddj", Age: 18 }
   - Trace: Other Person = { Name: "hdpjy", Age: 85 }
 ======================================
  !! Expectation Failed: Not aged 18
 ======================================
```
##### Irrelevant Objects Null Issues
Can't shrink/remove 'Other Turtle' because of the null property, see top of ObjectShrinkClassifier.

Check this with: 
* `InternalAffairs = [InternalAffairs.NullIssues]`  
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
   - Input: Turtle       = { Down: { Down: { Down: null } } }
   - Input: Other Turtle = { Down: { Down: null } }
   - Trace: Turtle       = { Down: { Down: { Down: null } } }
   - Trace: Other Turtle = { Down: { Down: null } }
   - WARNING: Null value for type 'Turtle' at input 'Turtle.Down.Down.Down'.
   - WARNING: Null value for type 'Turtle' at input 'Other Turtle.Down.Down'.
 ============================================================
  !! Expectation Failed: Turtle Ends on 'TurtlesAllTheWay'
 ============================================================
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
from verify in Trackr.Verify("Contains 3", () => observer.Contains(3)) // <= gen = [1, 3) : fails
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
from verify in Trackr.Verify("Rolls",
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
from stop in Trackr.StopWhenProven("Counter equals 3", () => counter == 3)
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
Using one input, one action, and one expectation,
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
   - Input: List                  = [ 5, 5 ]
   - Input: Item to Remove        = 5
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
from expect in Checkr.Expect("Returns true", () => output)
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
   - WARNING: All inputs were considered irrelevant.
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
from expect in Checkr.Expect("Returns true", () => output)
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
from expect in Checkr.Expect("Returns true", () => output)
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
    from expect in Checkr.Expect("RunInt s true", () => output)
    select Case.Closed,
    // --
    from str in Checkr.Input("String", Fuzzr.String())
    from output in Checkr.Act("BugHouse.RunString", () => bughouse.RunString(str))
    from expect in Checkr.Expect("RunString s true", () => output)
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
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: BugHouse.RunString (2 Times)
   - WARNING: All inputs were considered irrelevant.
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
   - Input: Item   = 3
   - Trace: Before = [ ]
   - Trace: After  = [ 3 ]
 ------------------------------------------------------------
  Executed: Add
   - Input: Item   = 3
   - Trace: Before = [ 3 ]
   - Trace: After  = [ 3, 3 ]
 ------------------------------------------------------------
  Executed: Remove
   - Input: Item   = 3
   - Trace: Before = [ 3, 3 ]
   - Trace: After  = [ 3 ]
 =====================================================
  !! Expectation Failed: List Does Not Contain Item
 =====================================================
 Passed Expectations
 - List Does Not Contain Item: 4x
 - List Contains Item: 4x
 ------------------------------------------------------------
```
## Various
### Execution Id Matters
#### The Sut
```csharp
public class ContactBook
{
    private readonly HashSet<string> contacts = [];
    public IReadOnlyCollection<string> Contacts => contacts;
    public void Add(string name) => contacts.Add(name);
    public void Update(string name, string newName)
    {
        contacts.Remove(name);
        Add(newName);
    }
}
```
#### The Checkr
```csharp
from contactBook in Trackr.Stashed(() => new ContactBook())
from stringPool in Trackr.PoolFor<string>()
from counter in Checkr.Input("Counter", Fuzzr.Counter("my counter"))
from operations in Checkr.OneOfWhen(
        Checkr.Option(() => counter == 1, // add AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 2, // update AAA to BBB, if this gets shrunk run fails on execution 3
            from newName in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from update in Trackr.OneOfPool<string>("Name to update", nameToUpdate =>
                from act in Checkr.Act("Update", () => contactBook.Update(nameToUpdate.Value, newName))
                from update in nameToUpdate.Replace(newName)
                select Case.Closed)
            select Case.Closed),
        Checkr.Option(() => counter == 3, // add a new AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 4, // add BBB, initial run fails here
            from name in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed))
from traceSut in Checkr.Trace("Sut", () => contactBook)
from count in Trackr.PoolExpect<string>("Count matches",
    a => a.Count() == contactBook.Contacts.Count)
select Case.Closed;
```
#### The Report (ShrinkMode.None)
```text
------------------------------------------------------------
 Test:                    RunTestNoShrinking
 Location:                Notes\X_Various\A_ExecutionIdMatters\A_ExecutionIdMatters.cs:23:1
 Original failing run:    4 executions
 Seed:                    342559779
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-1
   - Input: Counter = 1
   - Input: Name    = "AAA"
   - Trace: Sut     = { Contacts: [ "AAA" ] }
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: Counter        = 2
   - Input: Name           = "BBB"
   - Trace: Sut            = { Contacts: [ "BBB" ] }
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-2
   - Input: Counter = 3
   - Input: Name    = "AAA"
   - Trace: Sut     = { Contacts: [ "BBB", "AAA" ] }
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-3
   - Input: Counter = 4
   - Input: Name    = "BBB"
   - Trace: Sut     = { Contacts: [ "BBB", "AAA" ] }
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 3x
 ------------------------------------------------------------
```
Before this problem was identified, the following happened:
- Execution shrinker tries removing execution 2.
- The run now fails on execution 3.
- Run fails => execution 2 removed.
- Which means run no longer fails on execution 4
- Execution shrinker tries removing execution 4.
- The run still fails on execution 3.
- Run fails => execution 4 removed.
In this case it still pointed out a minimal fail case, just not the one that originally failed.  
But in other, more complex situations, this behaviour can lead to confusing reports.

So now, **Execution Id Matters**.
  
```text
------------------------------------------------------------
 Test:                    RunTestShrinking
 Location:                Notes\X_Various\A_ExecutionIdMatters\A_ExecutionIdMatters.cs:50:1
 Original failing run:    4 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1012196536
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-1
   - Input: Counter = 1
   - Trace: Sut     = { Contacts: [ "AAA" ] }
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: Counter        = 2
   - Input: Name           = "BBB"
   - Trace: Sut            = { Contacts: [ "BBB" ] }
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-3
   - Input: Counter = 4
   - Input: Name    = "BBB"
   - Trace: Sut     = { Contacts: [ "BBB" ] }
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 3x
 ------------------------------------------------------------
```
#### Also For Exceptions
```csharp
public class ContactBook
{
    private readonly List<string> contacts = [];
    public IReadOnlyCollection<string> Contacts => contacts;
    public void Add(string name)
    {
        if (contacts.Contains(name))
            ComputerSays.No(
                $"I'm afraid I can't do that {name.Split().FirstOrDefault() ?? name}");
        contacts.Add(name);
    }
    public void Update(string name, string newName)
    {
        contacts.Remove(name);
        Add(newName);
    }
}
```
```csharp
from contactBook in Trackr.Stashed(() => new ContactBook())
from stringPool in Trackr.PoolFor<string>()
from counter in Checkr.Input("Counter", Fuzzr.Counter("my counter"))
from operations in Checkr.OneOfWhen(
        Checkr.Option(() => counter == 1, // add AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 2, // update AAA to BBB, if this gets shrunk run fails on execution 3
            from newName in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from update in Trackr.OneOfPool<string>("Name to update", nameToUpdate =>
                from act in Checkr.Act("Update", () => contactBook.Update(nameToUpdate.Value, newName))
                from update in nameToUpdate.Replace(newName)
                select Case.Closed)
            select Case.Closed),
        Checkr.Option(() => counter == 3, // add a new AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 4, // add BBB
            from name in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed))
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\X_Various\A_ExecutionIdMatters\B_AlsoForExceptions.cs:20:1
 Original failing run:    4 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1954794658
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-1
   - Input: Counter = 1
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: Counter        = 2
   - Input: Name           = "BBB"
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-3
   - Input: Counter = 4
   - Input: Name    = "BBB"
 ===========================================================================
  !! QuickPulse.Instruments.ComputerSaysNo: I'm afraid I can't do that BBB
   at QuickPulse.Instruments.ComputerSays.No(String msg)
   at QuickCheckr.Tests.Notes.X_Various.A_ExecutionIdMatters.B_AlsoForExceptions.ContactBook.Add(String name) in Notes\X_Various\A_ExecutionIdMatters\B_AlsoForExceptions.cs:line 69
   at QuickCheckr.Tests.Notes.X_Various.A_ExecutionIdMatters.B_AlsoForExceptions.<>c__DisplayClass1_5.GetCheckr() in Notes\X_Various\A_ExecutionIdMatters\B_AlsoForExceptions.cs:line 55
   at QuickCheckr.Checkr.<>c__DisplayClass2_0.Act() in CheckrAct.cs:line 24
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.TryCatch() in CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
### Over Flowing
This looks like a redundant example. I think it started as a case for Trackr.Analyze. It isn't anymore, and does not look like a good one for that anyway.  
#### The SUT:
```csharp
public sealed class Ledger
{
    public readonly List<int> Entries = new();
    public void Add(int x) => Entries.Add(x);
    public int TotalChecked()
    {
        var sum = 0;
        foreach (var x in Entries) sum += x;
        return sum;
    }
}
```
#### The Checkr:
```csharp
from ledger in Trackr.Stashed(() => new Ledger())
from entry in Checkr.Input("Entry", Fuzzr.OneOf(
        (80, Fuzzr.Int(int.MaxValue / 2, int.MaxValue)),
        (20, Fuzzr.Int(1, 10))
    ))
from add in Checkr.Act("Add", () => ledger.Add(entry))
from expect in Trackr.Expect(
        "Checked total greater or equal than all entries", () =>
            ledger.Entries.All(a => ledger.TotalChecked() >= a))
select Case.Closed;
```
#### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\X_Various\B_OverFlowing\OverFlowing.cs:24:1
 Original failing run:    10 executions
 Minimal failing case:    2 executions (after 8 shrinks)
 Seed:                    1990927690
 ------------------------------------------------------------
  Executed: Add
   - Input: Entry = 1138084379
 ------------------------------------------------------------
  Executed: Add
   - Input: Entry = 1930233829
 ==========================================================================
  !! Expectation Failed: Checked total greater or equal than all entries
 ==========================================================================
```
### Trace Reports Original

**The Checkr:**  
```csharp
from x in Checkr.Input("x", Fuzzr.Int(-100, 100), Reduce.TowardsZero)
from trace in Checkr.Trace("x (trace)", () => x)
from expect in Checkr.Expect("x must be negative", () => x >= 0)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                TraceReportsOriginal.cs:37:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1468295090
 ------------------------------------------------------------
  Executed:
   - Input: x         = -12
            Redux       -1
   - Trace: x (trace) = -12
 =============================================
  !! Expectation Failed: x must be negative
 =============================================
```
### Shallow Object Copy

**The Model:**  
```csharp
public sealed class Box { public int Value { get; set; } }
```

**The Checkr:**  
```csharp
from box in Checkr.Input("box",
    from v in Fuzzr.Int(-10, 10)
    select new Box { Value = v }, Reduce.TowardsZero)
from expect in Checkr.Expect("Value must be >= 0", () => box.Value >= 0)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShallowObjectCopy.cs:54:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    458149048
 ------------------------------------------------------------
  Executed:
   - Input: box   = { Value: -9 }
            Redux   { Value: -1 }
 =============================================
  !! Expectation Failed: Value must be >= 0
 =============================================
```
### Coupled Predicate

**The Model:**  
```csharp
record Pair(int A, int B);
```

**The Checkr:**  
```csharp
from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(9, 9)))
from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_CoupledPredicate.cs:37:1
 Original failing run:    1 execution
 Seed:                    2017105380
 ------------------------------------------------------------
  Executed:
   - Input: pair = { A: 9, B: 9 }
 ======================================
  !! Expectation Failed: A + B <= 10
 ======================================
```
### Coupled Predicate

**The Model:**  
```csharp
public record Pair(int A, int B) { }
```

**The Checkr:**  
```csharp
from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(11, 11)), Reduce.TowardsZero)
from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CoupledPredicate.cs:37:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2017105380
 ------------------------------------------------------------
  Executed:
   - Input: pair  = { A: 11, B: 11 }
            Redux   { A: 0, B: 11 }
 ======================================
  !! Expectation Failed: A + B <= 10
 ======================================
```
### Parent Child Invariant

**The Model:**  
```csharp
record Account(int Balance, int CreditLimit);
```

**The Checkr:**  
```csharp
from account in Checkr.Input("pair", Fuzzr.Constant(new Account(-100, 50)))
from expect in Checkr.Expect("Balance >= -CreditLimit (i.e. never exceeds credit)",
     () => account.Balance >= -account.CreditLimit)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ParentChildInvariant.cs:54:1
 Original failing run:    1 execution
 Seed:                    483645165
 ------------------------------------------------------------
  Executed:
   - Input: pair = { Balance: -100, CreditLimit: 50 }
 ==============================================================================
  !! Expectation Failed: Balance >= -CreditLimit (i.e. never exceeds credit)
 ==============================================================================
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
from result in Checkr.Act("Eventually.AddOne", () => Eventually.AddOne(input))
from expect in Checkr.Expect("Is not 42", () => result != 42)
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
    from operations in Checkr.OneOfWhen(
        Trackr.PoolWhen<CoachBy>(a => a.Count() < 10, CreateCoach),
        Trackr.PoolWhen<CoachBy>(a => a.Any(), UpdateCoachSkills),
        Trackr.PoolWhen<CourseBy>(a => a.Count() < 5, CreateCourse),
        Trackr.PoolWhen<CourseBy>(a => a.Any(), UpdateCourseSkills),
        Trackr.PoolWhen<CoachBy>(a => a.Any() && dbCourses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
    from invariant in AssignedCoachesMustBeSuitable
    select Case.Closed;
```
**Create Coach Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from coach in Checkr.Act("Create Coach", () => service.CreateCoachAsync(name))
    from toPool in Trackr.ToPool("Store Coach", new CoachBy(name))
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCoachSkills =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from pool in Trackr.OneOfPool<CoachBy>("Coach to update", coach =>
        from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => service.UpdateCoachSkillsAsync(coach.Value.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Create Course Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCourse =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Course Name",
        from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
    from course in Checkr.Act("Create Course", () => service.CreateCourseAsync(name))
    from toPool in Trackr.ToPool("Store Course", new CourseBy(name))
    select Case.Closed;
```
**Update Course Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCourseSkills =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from pool in Trackr.OneOfPool<CourseBy>("Course to update", course =>
        from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Course Skills",
            () => service.UpdateCourseSkillsAsync(course.Value.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
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
 Location:                Notes\Y_Async\B_LockTheStableDoors\LockTheStableDoors.cs:31:1
 Original failing run:    13 executions
 Minimal failing case:    4 executions (after 14 shrinks)
 Seed:                    1511313504
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = CoachBy-2
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: Store Course = CourseBy-2
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Assign Coach to Course
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to update = CourseBy-2
   - Input: Course Skills    = [ _ ]
 ============================================================
  !! Expectation Failed: Assigned Coaches Must Be Suitable
 ============================================================
 Passed Expectations
 - Assigned Coaches Must Be Suitable: 5x
 ------------------------------------------------------------
```
### Async Act When

**The Model:**  
```csharp
public class Flip
{
    public static Task<bool> ThisFlag(Cell<bool> flag)
    {
        return Task.Run(() =>
        {
            flag.Value = !flag.Value;
            return flag.Value;
        });
    }
}
```

**The Checkr:**  
```csharp
from flag in Trackr.Stashed(() => new Cell<bool>(false))
from act in Checkr.ActWhen("Add", () => false, () => Flip.ThisFlag(flag))
from expect in Checkr.Expect("Fails when flag == False", () => flag.Value)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                AsyncActWhen.cs:29:1
 Original failing run:    1 execution
 Seed:                    254824330
 ------------------------------------------------------------
  Executed
 ===================================================
  !! Expectation Failed: Fails when flag == False
 ===================================================
```
## Registered But Unused
Because custom shrinkers can be defined for *nested* types, mistakes are possible.
Hence this warning.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Try.Values(["unused"]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                RegisteredButUnused.cs:59:1
 Original failing run:    1 execution
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
   - WARNING: Unused shrinker found for 'String' at key 'Input'.
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
### All Inputs Shrunk

**The Checkr:**  
```csharp
from input in Checkr.Input("An Input", Fuzzr.Int())
from expect in Checkr.Expect("Always fails", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                AllInputsShrunk.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1098916919
 ------------------------------------------------------------
  Executed:
   - WARNING: All inputs were considered irrelevant.
 =======================================
  !! Expectation Failed: Always fails
 =======================================
```
  
  
### Checkrs Court
The basic building block of QuickCheckr is a `CheckrOf<T>` (also, informally, called a *Checkr* throughout these docs). 

Multiple Checkrs can be combined into a composite Checkr using `LINQ` chains, as many of the examples show.  

The *atomic* Checkrs are created through a couple of static Factory classes :

- [The Checkr Class][TheCheckrClass]
  - Provides methods to create the *basic* Checkrs.
  - All of the Checkrs created by this class are *execution-scoped*.
- [The Tracker Class][TheTrackerClass]
  - Provides methods to create the Checkrs that deal with state-mutation, and/or Checkrs that are *run-scoped*.  
- [The Tracker Class][TheShowrClass]
  - Allows for customization of the QuickChecker value printer.  
  

[TheCheckrClass]: #the-checkr-class

[TheTrackerClass]: #the-tracker-class

[A_TheShowrClass]: #the-showr-class
#### The Checkr Class
##### Input
`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.  

**Usage**  
```csharp
Checkr.Input("AnInt", Fuzzr.Int());
```
####### Exception Handling
The Checkr below will throw a QuickFuzzr `OneOfEmptyOptionsException`:  
```csharp
from input in Checkr.Input("AnInt", Fuzzr.OneOf<int>((List<int>)[]))
select Case.Closed;
```
Whenever an input throws, the exception is wrapped into a `InputFuzzrThrewException`.  
```csharp
Input 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state.
And as 'Input' is eagerly evaluated, this can cause problems.
In the latter case please try using the 'lazy' version. 
Example: 'Checkr.Input(() => Fuzzr.Int()).
```
As you can gather from the exception message, this is done in order to inform the user of a rare, but real,
issue when a Fuzzr depends on external state.  
The solution is also supplied in said message:   
```csharp
Checkr.Input("AnInt", () => Fuzzr.Int());
```
If the *lazy* version throws:  
```csharp
from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
select Case.Closed;
```
The Exception is **not** wrapped.  
```csharp
Fuzzr.OneOf<Int32> cannot select from an empty sequence.
Possible solutions:
- Provide at least one option (ensure the sequence is non-empty).
- Use a fallback: Fuzzr.OneOf(values).WithDefault()
- Guard upstream: values.Any() ? Fuzzr.OneOf(values) : Fuzzr.Constant(default!).
```
#### The Tracker Class
### Conditional Execution
#### Checkr Act Carefully When

**The Checkr:**  
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
from thing in Checkr.ActCarefullyWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
from notNull in Checkr.Expect("Not Null", () => thing.HasValue)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CheckrActCarefullyWhen.cs:50:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    625676917
 ------------------------------------------------------------
  Executed: Act
   - Input: Counter = 3
 ===================================
  !! Expectation Failed: Not Null
 ===================================
 Passed Expectations
 - Not Null: 1x
 ------------------------------------------------------------
```
#### Checkr Act When
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
from never in Checkr.ActWhen("Never", () => false, () => { })
from neverAgain in Checkr.ActWhen("Never Again", () => false, () => true)
from thing in Checkr.ActWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
select Case.Closed;
```
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
from never in Checkr.ActWhen("Never", () => false, () => Task.CompletedTask)
from neverAgain in Checkr.ActWhen("Never Again", () => false, () => Task.FromResult(true))
from thing in Checkr.ActWhen("Act", () => counter != 2, () => Task.FromResult(counter == 3 ? null : new Thing()))
from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
select Case.Closed;
```
#### Checkr When
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
from thing in Checkr.When(() => counter != 2, Checkr.Act("Act", () => counter == 3 ? null : new Thing()))
from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
select Case.Closed;
```
### When Things Go Wrong
#### Everything Means Less Than Null
##### Trackr To Pool
```csharp
from one in Trackr.ToPool("answer", 42)
select Case.Closed;
```
  Throws:  
```text
A storage attempt was made for a 'Pool' of 'System.Int32' using key 'answer', but no such pool was found.
Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
```
#### It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
##### Checkr Act
```csharp
from one in Checkr.Act("reused", () => { })
from two in Checkr.Act("reused", () => { })
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
##### Checkr Act Carefully
```csharp
from one in Checkr.ActCarefully("reused", () => { })
from two in Checkr.ActCarefully("reused", () => { })
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
##### Checkr Expect
```csharp
from one in Checkr.Expect("reused", () => true)
from two in Checkr.Expect("reused", () => true)
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
##### Checkr Input
```csharp
from one in Checkr.Input("reused", Fuzzr.Int())
from two in Checkr.Input("reused", Fuzzr.Int())
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Input'
```
##### Checkr One Of
```csharp
from one in Checkr.OneOf(Checkr.Act("One", () => { }))
from two in Checkr.OneOf(Checkr.Act("Two", () => { }))
select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
from one in Checkr.OneOf("reused", Checkr.Act("One", () => { }))
from two in Checkr.OneOf("reused", Checkr.Act("Two", () => { }))
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
##### Checkr One Of When
```csharp
from one in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("One", () => { })))
from two in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("Two", () => { })))
select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOfWhen|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
from one in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("One", () => { })))
from two in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("Two", () => { })))
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
##### Checkr Trace
```csharp
from one in Checkr.Trace("reused", () => "")
from two in Checkr.Trace("reused", () => "")
from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
##### Checkr Trace When
```csharp
from one in Checkr.TraceWhen("reused", () => true, () => "")
from two in Checkr.TraceWhen("reused", () => true, () => "")
from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
##### Trackr Expect
```csharp
from one in Trackr.Expect("reused", () => true)
from two in Trackr.Expect("reused", () => true)
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
##### Trackr Pool For
```csharp
from one in Trackr.PoolFor<int>()
from two in Trackr.PoolFor<int>()
select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.PoolFor'
```
##### Trackr Stashed
```csharp
from one in Trackr.Stashed(() => 42)
from two in Trackr.Stashed(() => 666)
select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
```csharp
from one in Trackr.Stashed("reused", () => 42)
from two in Trackr.Stashed("reused", () => 666)
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
##### Trackr Stop When Proven
```csharp
from one in Trackr.StopWhenProven("reused", () => false)
from two in Trackr.StopWhenProven("reused", () => false)
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
##### Trackr Verify
```csharp
from one in Trackr.Verify("reused", () => true)
from two in Trackr.Verify("reused", () => true)
select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
### Issues
#### The Unshrinkables
##### Unknown Primitives
```csharp
from unkown in Checkr.Input("Half", Fuzzr.Half())
from fail in Checkr.Expect("Fails", () => false)
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\Z_Issues\A_TheUnshrinkables\B_UnknownPrimitives\UnknownPrimitives.cs:19:1
 Original failing run:    1 execution
 Seed:                    746257211
 ------------------------------------------------------------
  Executed:
   - Input: Half = 65,25
   - WARNING: Can't shrink type 'Half' at input 'Half'.
 ================================
  !! Expectation Failed: Fails
 ================================
```
#### System Types
```csharp
public class Thing
{
    public int Irrelevant { get; set; }
    public Type Type { get; set; } = typeof(int);
    public Type OtherType { get; set; } = typeof(int);
}
```
```csharp
from thing in Checkr.Input("A thing with a type", Fuzzr.Constant(new Thing()))
from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1487819190
 ------------------------------------------------------------
  Executed:
   - Input: A thing with a type = { Type: Int32, OtherType: Int32 }
   - WARNING: Object of type 'Type' has no shrinkable properties at input 'A thing with a type.Type'.
   - WARNING: Object of type 'Type' has no shrinkable properties at input 'A thing with a type.OtherType'.
 ================================
  !! Expectation Failed: Fails
 ================================
```
###### Workaround
```csharp
from thing in Checkr.Input("A thing with a type",
    Fuzzr.Constant(new Thing()),
    Try.Values([typeof(string), typeof(double)]))
from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTestSolution
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    350297528
 ------------------------------------------------------------
  Executed:
   - Input: A thing with a type = { Type: Int32 }
 ================================
  !! Expectation Failed: Fails
 ================================
```
