# Registered But Unused
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
## All Inputs Shrunk

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
  
  
## Building a Testing DSL with QuickCheckr
### Bolting Horses Dsl

**The Invariant:**  
```csharp
SystemCheckr
    .Named("Bolting Horses")
    .DisableWarnings()
    .PoolOf<Coach>()
    .PoolOf<Course>()
    .Operations(
        new CreateCoach(),
        new UpdateCoachSkills(),
        new CreateCourse(),
        new UpdateCourseSkills(),
        new AssignCoachToCourse())
    .ForAll<Coach>("All Assigned Coaches Must Be Suitable",
        coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
    .Run(1473455315, 20.ExecutionsPerRun());
```
```csharp
public class CreateCoach : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
        system.Create<Coach>()
            .When(coaches => coaches.Count <= 10)
            .With(from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
            .Perform(input => new Coach(input))
            .Expect(("Name", (coach, input) => coach.Name == input));
}
```
```csharp
public class UpdateCoachSkills : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
        system.Update<Coach>("Skills")
            .When(coaches => coaches.Count != 0)
            .With(Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            .Perform((coach, input) => coach.UpdateSkills(input))
            .Expect(("Skills", (coach, input) =>
                coach.Skills.Count == input.Count() && coach.Skills.All(input.Contains)));
}
```
```csharp
public class CreateCourse : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
        system.Create<Course>()
            .When(courses => courses.Count <= 5)
            .With(from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
            .Perform(input => new Course(input))
            .Expect(("Name", (course, input) => course.Name == input));
}
```
```csharp
public class UpdateCourseSkills : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
        system.Update<Course>("Skills")
            .When(courses => courses.Count != 0)
            .With(Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            .Perform((course, input) => course.UpdateRequiredSkills(input))
            .Expect(("Skills", (course, input) =>
                course.RequiredSkills.Count == input.Count() && course.RequiredSkills.All(input.Contains)));
}
```
```csharp
public class AssignCoachToCourse : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
        system.Update<Coach, Course>("Assign")
            .When((coach, course) => coach.IsSuitableFor(course))
            .Perform((coach, course) => course.AssignCoach(coach))
            .Expect(("Assigned", (coach, course) => course.AssignedCoach == coach));
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                BoltingHorsesDsl.cs:53:1
 Original failing run:    13 executions
 Minimal failing case:    4 executions (after 14 shrinks)
 Seed:                    1473455315
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: 'Create Course' to Pool = Course-1
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: 'Create Coach' to Pool = Coach-1
 ------------------------------------------------------------
  Executed: Update (Coach, Course) Assign
   -  Pool: Update (Coach, Course) Assign = ( Coach-1, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Update Course Skills         = Course-1
   - Input: 'Update Course Skills' Input = [ _ ]
 ================================================================
  !! Expectation Failed: All Assigned Coaches Must Be Suitable
 ================================================================
 Passed Expectations
 - 'Create Course' Name: 4x
 - All Assigned Coaches Must Be Suitable: 12x
 - 'Create Coach' Name: 4x
 - 'Update Coach Skills' Skills: 2x
 - 'Update (Coach, Course) Assign' Assigned: 2x
 - 'Update Course Skills' Skills: 1x
 ------------------------------------------------------------
```
### Going Functional
#### Spike

**The Model:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. FudgeIt(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> FudgeIt(IEnumerable<int> list)
        => list.Where(a => a != 6).Concat(list.Where(a => a == 6));
}
```

**The Invariant:**  
```csharp
Check.ForAll(
    from len in Fuzzr.Int(0, 10)
    from elements in Fuzzr.Int(0, 10).Many(len)
    select elements.ToList(),
    list => list.SequenceEqual(TheReverser.Reverses(TheReverser.Reverses(list))));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                GoingFunctional.cs:27:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    2116954181
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, 6 ]
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Simplest

**The Invariant:**  
```csharp
Check.ForAll(Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Simplest.cs:24:1
 Original failing run:    1 execution
 Seed:                    924452929
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Seeded

**The Invariant:**  
```csharp
Check.ForAll(1948674479, Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_Seeded.cs:26:1
 Original failing run:    1 execution
 Seed:                    1948674479
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Specifying Runs

**The Invariant:**  
```csharp
Check.ForAll(200.Runs(), Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_SpecifyingRuns.cs:24:1
 Original failing run:    1 execution
 Seed:                    823515705
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### With Classifiers

**The Invariant:**  
```csharp
Check.ForAll(Fuzzr.Constant(7), a => a == 6, Try.Default<int>(), Try.Values(6));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_WithClassifiers.cs:24:1
 Original failing run:    1 execution
 Seed:                    1830629706
 ------------------------------------------------------------
  Executed:
   - Input: Input = 7
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Applies Default Reducers

**The Invariant:**  
```csharp
Check.ForAll(1429680296, Fuzzr.Int(0, 10), a => a < 2);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                F_AppliesDefaultReducers.cs:24:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1429680296
 ------------------------------------------------------------
  Executed:
   - Input: Input = 5
            Redux   2
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### With Classifiers Seeded Showing Redux Too

**The Invariant:**  
```csharp
Check.ForAll(893841622, Fuzzr.Int(0, 10), a => a == 6, Try.Default<int>(), Try.Values(6));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                G_WithClassifiersSeededShowingReduxToo.cs:24:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    893841622
 ------------------------------------------------------------
  Executed:
   - Input: Input = 9
            Redux   7
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### With Custom Reducer

**The Invariant:**  
```csharp
Check.ForAll(257911700, Fuzzr.Int(0, 10), a => a != 6, Reduce.Values([0, 6]));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H_WithCustomReducer.cs:24:1
 Original failing run:    1 execution
 Seed:                    257911700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
### Using Linq

**The Model:**  
```csharp
public class Model
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Sut
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }
    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
```

**The Checkr:**  
```csharp
from model in Trackr.Stashed(() => new Model())
from sut in Trackr.Stashed(() => new Sut())
from ops in Checkr.OneOf(
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Add", () =>
    {
        model.Add(input);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_UsingLinq.cs:72:1
 Original failing run:    14 executions
 Minimal failing case:    4 executions (after 13 shrinks)
 Seed:                    1299945702
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 196.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 13x
 ------------------------------------------------------------
```
### Using Dsl

**The Model:**  
```csharp
public class Model
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Sut
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }
    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
```

**The Checkr:**  
```csharp
 Check
    .Model(() => new Model())
    .Sut(() => new Sut())
    .Operation("Add", Fuzzr.Int(),
        (model, sut, a) =>
        {
            model.Add(a);
            sut.Add(a);
        })
    .Operation("Subtract", Fuzzr.Int(),
        (model, sut, a) =>
        {
            model.Subtract(a);
            sut.Subtract(a);
        })
    .Operation("Clear",
        (model, sut) =>
        {
            model.Clear();
            sut.Clear();
        })
    .Observe(
        (model, sut) => model.Result == sut.Result,
        (model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.");
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_UsingDsl.cs:72:1
 Original failing run:    7 executions
 Minimal failing case:    4 executions (after 6 shrinks)
 Seed:                    1879928073
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 193.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 6x
 ------------------------------------------------------------
```
### Model Based Testing
## Checkrs Court
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
### The Checkr Class
#### Input
`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.  

**Usage**  
```csharp
Checkr.Input("AnInt", Fuzzr.Int());
```
###### Exception Handling
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
### The Tracker Class
## Conditional Execution
### Checkr Act Carefully When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActCarefullyWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
### Checkr Act When
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
### Checkr When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.When(() => counter != 2, Checkr.Act("Act", () => counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
## When Things Go Wrong
### Everything Means Less Than Null
#### Trackr To Pool
```csharp
    from one in Trackr.ToPool("answer", 42)
    select Case.Closed;
```
  Throws:  
```text
A storage attempt was made for a 'Pool' of 'System.Int32' using key 'answer', but no such pool was found.
Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
```
### It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
#### Checkr Act
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
#### Checkr Act Carefully
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
#### Checkr Expect
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
#### Checkr Input
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
#### Checkr One Of
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
#### Checkr One Of When
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
#### Checkr Trace
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
#### Checkr Trace When
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
#### Trackr Expect
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
#### Trackr Pool For
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
#### Trackr Stashed
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
#### Trackr Stop When Proven
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
#### Trackr Verify
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
## Issues
### The Unshrinkables
#### Unknown Primitives
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
### System Types
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
##### Workaround
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
