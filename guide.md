# Guide

Welcome to the QuickCheckr Guide.  
This is a living document, we start small, with a short introduction and a real-world example.  
## Contents

- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.  
## Coming Soon
More chapters will follow as QuickCheckr grows:  
- The Stash, Runs and Executions
- Shrinking 101
- Pools
  

[CheckringThingsOut]: #checkr-ing-things-out

[BoltingHorses]: #bolting-horses
## Checkr-ing Things Out
A *Checkr* describes a sequence of steps that QuickCheckr will execute repeatedly.
Each run goes through the script from top to bottom: generating inputs, performing actions, checking specs, and collecting diagnostics.
If any execution fails, QuickCheckr shrinks it to the smallest, easiest-to-understand failing case.  
### First Specification
Let's start by having a look at the simplest possible Checkr:  
```csharp
public class MinimalCheckr
{
    [Fact]
    public void RunTest()
        => Checkr.Spec("Always true", () => true).Run();
}
```
Congratulations! You've just checked something out. The test passes and now we know that `true` is always `true`.

So what happens if try to do the opposite?  
```csharp
Checkr.Spec("Always true", () => false);
```
The test fails and QuickCheckr produces the following report:  
```text
------------------------------------------------------------
 Test:                    SpecAlwaysFalse
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:42:1
 Original failing run:    1 execution
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed (1)
 ===============================
  !! Spec Failed: Always true
 ===============================
```

The report shows:
* A run summary
  * Test: Name of the test executing the Checkr.
  * Location: Source code Location of the test executing the Checkr.
  * Original failing run: Number of executions executed during the run. Let me get back to you on that one.
  * Seed: Seed used to generate the failing run. You can use this to reproduce the failing run reliably.
* The minimal fail case:  The minimal failing input sequence. In this case original run and minimal run are the same.  
  And details, also in this case, consist only of one execution with it's ID in parentesis.
* Failure details: Why the run failed in the first place.
  
### Gathering Input
Let's add an input to the Checkr, and change our spec:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from spec in Checkr.Spec("Input /= 42", () => input != 42)
    select Case.Closed;
```

*Note:* `Case.Closed` marks the end of the Checkr script.
A Checkr is just a LINQ expression that evaluates step by step,
and the final select clause must return a value representing the completed case.
`Case.Closed` simply signals "this case finished successfully, there are no more steps".

QuickCheckr relies on the [`QuickFuzzr`](https://github.com/kilfour/QuickFuzzr/blob/main/README.md) library to generate inputs.

`Fuzzr.Int()` generates a random integer between 1 and 100, so this test will fail, on avarage, once every hundred times.  
Not ideal, so let's up the run number, like so:  
```csharp
check.Run(200.Runs());
```
Now the test fails consistently:  
```text
------------------------------------------------------------
 Test:                    AddingAnInput
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:81:1
 Original failing run:    1 execution
 Seed:                    127648209
 ------------------------------------------------------------
  Executed (1):
   - Input: Input = 42
 ===============================
  !! Spec Failed: Input /= 42
 ===============================
```
As you can see the input is now included in the report *minimal fail case*,
this is because QuickCheckr judged the input to be relevant to the failure.  
So, if we add another input to the Checkr, one which does not influence the failure:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Spec("Input /= 42", () => input != 42)
    select Case.Closed;
```
The following happens:  
```text
------------------------------------------------------------
 Test:                    AddingAnotherInput
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:121:1
 Original failing run:    1 execution
 Seed:                    127648209
 ------------------------------------------------------------
  Executed (1):
   - Input: Input = 42
 ===============================
  !! Spec Failed: Input /= 42
 ===============================
```
That's basically the same thing as before, because QuickCheckr does not report irrelevant inputs.  
### Not Without a Trace
QuickCheckr's *shrinking* can sometimes seem magical. Not a problem in this simple test,
but when you are writing a more complex test, it is always helpful to see what's really going on.

QuickCheckr is built with transparancy and discoverability as a first class concern:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Spec("Input /= 42", () => input != 42)
    from trace in Checkr.Trace("Irrelevant Input", () => irrelevant)
    select Case.Closed;
```
By adding a `Checkr.Trace` to the `LINQ` chain, we get the following report:  
```text
------------------------------------------------------------
 Test:                    NotWithoutATrace
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:156:1
 Original failing run:    1 execution
 Seed:                    769641533
 ------------------------------------------------------------
  Executed (1):
   - Input: Input = 42
   - Trace: Irrelevant Input = 47
 ===============================
  !! Spec Failed: Input /= 42
 ===============================
```
### Time to Act!
Testing inputs is fun and all, but really, we want to test some kind of behaviour, right?  
```csharp
var check =
    from one in Checkr.Input("One", Fuzzr.Int())
    from two in Checkr.Input("Two", Fuzzr.Int())
    from three in Checkr.Input("Three", Fuzzr.Int())
    from sum in Checkr.Act("One plus Three", () => one + three)
    from spec in Checkr.Spec("Sum /= 42", () => sum != 42)
    select Case.Closed;
```
Still a trivial example, but it shows how you can use `Checkr.Act` to start mutating things.  
And here's the report:  
```text
------------------------------------------------------------
 Test:                    TimeToAct
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:193:1
 Original failing run:    1 execution
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed (1): One plus Three
   - Input: One = 24
   - Input: Three = 18
 =============================
  !! Spec Failed: Sum /= 42
 =============================
```
As demonstrated, the name of the executed action is included in the report, as well as the relevant input.  
## Bolting Horses
For this example we are going the use a simplified version of the [`HorsesForCourses`](https://github.com/kilfour/HorsesForCourses/blob/main/README.md) domain.  
The bug we're hunting was actually found in the wild (although I simplified the code, in order to keep it short)
and is demonstrative for a whole category: *Invariants drifting over time*.
  
**The Coach:**  
```csharp
public class Coach(string name)
{
    public string Name { get; init; } = name;
    public IReadOnlySet<string> Skills => skills;
    private readonly HashSet<string> skills = [];
    public IReadOnlyList<Course> AssignedCourses => assignedCourses;
    private readonly List<Course> assignedCourses = [];
    public void UpdateSkills(IEnumerable<string> newSkills)
    {
        skills.Clear();
        foreach (var s in newSkills) skills.Add(s);
    }
    public bool IsSuitableFor(Course course) => course.RequiredSkills.All(Skills.Contains);
    public void AssignCourse(Course course) => assignedCourses.Add(course);
}
```
**The Course:**  
```csharp
public class Course(string name)
{
    public string Name { get; init; } = name;
    public IReadOnlySet<string> RequiredSkills => requiredSkills;
    private readonly HashSet<string> requiredSkills = [];
    public Coach? AssignedCoach { get; private set; }
    public void UpdateRequiredSkills(IEnumerable<string> newSkills)
    {
        requiredSkills.Clear();
        foreach (var s in newSkills) requiredSkills.Add(s);
    }
    public void AssignCoach(Coach coach)
    {
        if (!coach.IsSuitableFor(this))
            throw new InvalidOperationException("Coach does not have the required skills");
        AssignedCoach = coach;
        coach.AssignCourse(this);
    }
}
```
**The Checkr:**  
This one is the *entry point* for the test.  
It creates two pools, one for coaches, and one for courses, which we are going to use to track the state of the system.  
It then creates a series of actions, randomly choosing one, using `Checkr.OneOfIf`, out of those in the list for which it's predicate is fulfilled.
Finally after each action we check the `AssignedCoachesMustBeSuitable` Invariant.  
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from coaches in Trackr.Pool<Coach>()
    from courses in Trackr.Pool<Course>()
    from operations in Checkr.OneOfIf(
        (() => coaches.Count() < 10, CreateCoach),
        (() => coaches.Any(), UpdateCoachSkills),
        (() => courses.Count() < 5, CreateCourse),
        (() => courses.Any(), UpdateCourseSkills),
        (() => coaches.Any() && courses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
    from invariant in AssignedCoachesMustBeSuitable
    select Case.Closed;
```
**Create Coach Checkr:**  
In this one we create a coach with a random name, verify that the property `Name` is assigned and then add it to the coaches pool.  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from trace in Checkr.Trace("Coach Name", () => name)
    from coach in Checkr.Act("Create Coach", () => new Coach(name))
    from checkName in Checkr.Spec("Coach Name is set", () => coach.Name == name)
    from toPool in Trackr.Pool("Store Coach", coach)
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
Now we retrieve a random coach from the pool and update its skills, verifying that the property `Skills` is set correctly.  
```csharp
private static readonly CheckrOf<Case> UpdateCoachSkills =
    from coach in Trackr.OneOfPool<Coach>("Coach to update")
    from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from valid in Checkr.When(() => coach is not null,
        from trace in Checkr.Trace("Coach Name", () => coach.Name)
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => coach.UpdateSkills(skills))
        from checkSkills in Checkr.Spec("Coach Skills Set",
            () => coach.Skills.Count == skills.Count() && coach.Skills.All(skills.Contains))
        select Case.Closed)
    select Case.Closed;
```
**Create Course Checkr:**  
Same pattern as the *Create Coach Checkr*.  
```csharp
private static readonly CheckrOf<Case> CreateCourse =
    from name in Checkr.Input("Course Name",
        from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
    from trace in Checkr.Trace("Course Name", () => name)
    from course in Checkr.Act("Create Course", () => new Course(name))
    from checkName in Checkr.Spec("Course Name Set", () => course.Name == name)
    from toPool in Trackr.Pool("Store Course", course)
    select Case.Closed;
```
**Update Course Skills Checkr:**  
And again, same pattern as the *Update Coach Skills Checkr*.  
```csharp
private static readonly CheckrOf<Case> UpdateCourseSkills =
    from course in Trackr.OneOfPool<Course>("Course to update")
    from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from courseNotNull in Checkr.When(() => course is not null,
        from traceCourse in Checkr.Trace("Course Name", () => course.Name)
        from updateSkills in Checkr.Act("Update Course Skills",
            () => course.UpdateRequiredSkills(skills))
        from checkSkills in Checkr.Spec("Course Required Skills Set",
            () => course.RequiredSkills.Count == skills.Count() && course.RequiredSkills.All(skills.Contains))
        select Case.Closed)
    select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
And finally here's the system invariant. Any coach assigned a course must have the required skills to teach that course.  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from coaches in Trackr.Pool<Coach>()
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
 Location:                Docs\A_Guide\E_HorsesAgain\BoltingHorses.cs:54:1
 Original failing run:    20 executions
 Minimal failing case:    4 executions (after 16 shrinks)
 Seed:                    13918324
 ------------------------------------------------------------
  Executed (8): Create Coach
   - Trace: Coach Name = Coach-5
 ------------------------------------------------------------
  Executed (10): Create Course
   - Trace: Course Name = Course-3
 ------------------------------------------------------------
  Executed (13): Assign Coach to Course
   - Trace: Coach Name = Coach-5
   - Trace: Course Name = Course-3
 ------------------------------------------------------------
  Executed (20): Update Course Skills
   - Input: Course Skills = [ _ ]
   - Trace: Course Name = Course-3
 =====================================================
  !! Spec Failed: Assigned Coaches Must Be Suitable
 =====================================================
 Passed Specs
 - Course Name Set: 4x
 - Coach Name is set: 9x
 - Coach Skills Set: 2x
 - Coach is assigned: 4x
 - Assigned Coaches Must Be Suitable: 9x
 - Course Required Skills Set: 1x
 ------------------------------------------------------------
```
