# Guide

Welcome to the QuickCheckr Guide.  
This is a living document, we start small, with a short introduction and a real-world example.  
## Contents

- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [The Stash, Runs and executions][TheStashRunsAndExecutions]
  -  An example and an explanation of how QuickCheckr handles stateful testing.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.  

[CheckringThingsOut]: #checkr-ing-things-out

[TheStashRunsAndExecutions]: #the-stash-runs-and-executions

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
  Executed
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
* The minimal fail case: In this toyest of toy examples it *only* specifies that an execution happened.
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

`Fuzzr.Int()` generates a random integer between 1 and 100, so this test will fail, on average, once every hundred times.  
Not ideal, so let's up the run number, like so:  
```csharp
check.Run(200.Runs());
```
Now the test fails consistently:  
```text
------------------------------------------------------------
 Test:                    AddingAnInput
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:80:1
 Original failing run:    1 execution
 Seed:                    127648209
 ------------------------------------------------------------
  Executed:
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:122:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    127648209
 ------------------------------------------------------------
  Executed:
   - Input: Input = 42
 ===============================
  !! Spec Failed: Input /= 42
 ===============================
```
That's basically the same thing as before, because QuickCheckr does not report irrelevant inputs.  
### Not Without a Trace
QuickCheckr's *shrinking* can sometimes seem magical. Not a problem in this simple test,
but when you are writing a more complex test, it is always helpful to see what's really going on.

QuickCheckr is built with transparency and discoverability as a first class concern:  
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:159:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    769641533
 ------------------------------------------------------------
  Executed:
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:198:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed: One plus Three
   - Input: One = 24
   - Input: Three = 18
 =============================
  !! Spec Failed: Sum /= 42
 =============================
```
As demonstrated, the name of the executed action is included in the report, as well as the relevant inputs.  
## The Stash, Runs and Executions
Told you I was going to get back to you on that one (executions).

In the previous chapter we upped the run number in order to *fully explore the input space*.  
But what if it's not the input that causes the issue, but a side-effect.  
Consider:  
```csharp
public class BugHouse
{
    private int count;
    public bool DelayedDetonation(int a)
    {
        if (count++ == 2 && a == 1) ComputerSays.No("I'm sorry, Dave, I'm afraid I can't do that.");
        return true;
    }
}
```
If we want to test it and write a Checkr like so:  
```csharp
private static readonly CheckrOf<Case> FaultyBugHouseCheckr =
    from input in Checkr.Input("Int", Fuzzr.Int())
    from callBugHouse in Checkr.Act("BugHouse.DelayedDetonation", () => new BugHouse().DelayedDetonation(input))
    from checkResult in Checkr.Spec("Returns true", () => callBugHouse)
    select Case.Closed;
```
The test will always pass, even if we increase the amount of runs to infinity,
because the test looses the state of the `BugHouse` between every run.  
So here's what we do:
1. We stash the `BugHouse` using `Trackr.Stashed` and instead of creating a new one in the `Act` we use the stashed one.
2. We up the number of executions per run.  
   The contents of `Trackr.Stashed` is reset after every run, but not in between executions.
   This allows us to *capture* the state of the `BugHouse`.  
   
**The Updated Checkr:**  
```csharp
private static readonly CheckrOf<Case> BugHouseCheckr =
    from bugHouse in Trackr.Stashed(() => new BugHouse())
    from input in Checkr.Input("Int", Fuzzr.Int())
    from callBugHouse in Checkr.Act("BugHouse.DelayedDetonation", () => bugHouse.DelayedDetonation(input))
    from checkResult in Checkr.Spec("Returns true", () => callBugHouse)
    select Case.Closed;
```
**Running It:**  
```csharp
BugHouseCheckr.Run(100.Runs(), 10.ExecutionsPerRun());
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunBugHouseCheckr
 Location:                Docs\A_Guide\B_TheStashRunsAndExecutions\TheStashRunsAndExecutions.cs:62:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1854571642
 ------------------------------------------------------------
  Executed: BugHouse.DelayedDetonation (2 Times)
 ------------------------------------------------------------
  Executed: BugHouse.DelayedDetonation
   - Input: Int = 1
 ===========================================================================
  !! Exception Thrown: ComputerSaysNo: I'm sorry, Dave, I'm afraid I can't do that.
 ===========================================================================
 Passed Specs
 - Returns true: 2x
 ------------------------------------------------------------
```
The shrunk case makes it immediately clear why this Checkr needed multiple executions.  

QuickCheckr shows that the action BugHouse.DelayedDetonation was executed twice without issue,
and only on the third execution, where the relevant input `Int = 1` appears, does the exception occur.  

Because the BugHouse instance is stashed, its internal count persisted across executions, allowing the failure to surface.  

The shrinking process confirms that all three executions are required:
removing either of the first two would prevent the internal state from reaching the dangerous value.  

Finally, the report closes by listing which specs passed during the original run,
highlighting that the behaviour was stable until exactly the offending execution.  
### Recap
QuickCheckr operates across *two distinct scopes*, and understanding them is essential when testing stateful systems:

**1. Run Scope**

A *run* is a full pass through your Checkr script.
Every run starts with a clean slate:

* All `Trackr.Stashed` values are reset.
* Pools (`Trackr.Pool`, explained later) start empty.
* Counters, uniqueness constraints, and any internal memories tied to a run are cleared.

This guarantees that each run is **independent**, **repeatable**, and **reproducible** based on the seed.

**But inside a run**, QuickCheckr may execute your script multiple times…

### **2. Execution Scope**

An *execution* is one walk through the Checkr script *within* a single run.
Executions share:

* The **same stashed instances** (e.g. the `BugHouse`).
* The **same pools**, which accumulate objects over executions.
* The **effects** of all previous executions in the run.

Meanwhile, the following *are fresh each execution*:

* Every `Checkr.Input` value.
* Every `Checkr.Act`, `Checkr.Spec`, and `Checkr.Trace` is evaluated anew.
* The shrinking engine can re-run isolated executions to evaluate relevance.

This means:

* **Runs give you independence.**
* **Executions give you statefulness.**

And together, they let QuickCheckr explore both *pure input variation* and *multi-step behaviour*,
which is exactly what makes the BugHouse example catchable:
the failure only emerges when several executions act on the *same* object.  
## Choosing The Next Move
In the previous chapter we mentioned *Pools* and promised to explain how they work.  
We'll get there soon, but first we need to talk about `Checkr.OneOf`, the tool that lets QuickCheckr explore branching behaviour.

`OneOf` is QuickCheckr's way of saying:

> **"At this step, choose *exactly one* of these possible behaviours and execute it."**

It is the foundational building block for introducing randomness into *what happens next*.
Instead of describing one action, you describe a *set* of alternative next moves,
and QuickCheckr picks one at random during each execution.

Let's keep the model simple:  
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```
Suppose our system has a (not implemented here) 'no overdraft' invariant.
The bug is trivial, but that's fine: we're here to demonstrate *behavioural branching*, not banking security.  
### A Checkr With a Choice
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int())
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
Every execution through this Checkr does the same things:

1. Generate a random amount.
2. Pick exactly **one** action: `Deposit` *or* `Withdraw`.
3. Check the invariant.

A key detail:

> **Each branch runs inside the same execution scope.**
> This means both branches mutate the same stashed `account` instance.

So across executions, QuickCheckr builds a sequence of deposits and withdrawals, any of which may lead to a failure.
  
### Seeing Which Branches Were Chosen
Running the Checkr without shrinking yields a report similar to this:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:47:1
 Original failing run:    8 executions
 Seed:                    1169776765
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 71
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 5
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 31
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 15
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 56
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 94
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 36
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 66
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 7x
 ------------------------------------------------------------
```
This shows us something useful:  
**QuickCheckr records the branch actually taken for each execution.**  
There is no guesswork.  
### Shrinking Branching Behaviour
And if we let the QuickCheckr shrinker do its work:  
```text
------------------------------------------------------------
 Test:                    RunTest_Shrunk
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:146:1
 Original failing run:    8 executions
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    1169776765
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 66
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 7x
 ------------------------------------------------------------
```
The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
* The amount was positive, so withdrawing it immediately violates the invariant.

This highlights an important point:

> **QuickCheckr does not explore alternative branches during shrinking.**  
> It only shrinks *what actually happened*.

Shrinking removes irrelevant **executions**, trims **inputs**, and discards everything that had no influence on the failure.
The branch chosen during the original failing run is *the* branch examined during shrinking.   
### Recap
`Checkr.OneOf` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.
* Shrinking works naturally: QuickCheckr does **not** try unchosen branches. 
  It only minimizes the actual failing scenario.  
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
public static readonly CheckrOf<Case> TheCheckr =
    from coaches in Trackr.PoolFor<Coach>()
    from courses in Trackr.PoolFor<Course>()
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
 Location:                Docs\A_Guide\E_HorsesAgain\BoltingHorses.cs:59:1
 Original failing run:    20 executions
 Minimal failing case:    4 executions (after 21 shrinks)
 Seed:                    13918324
 ------------------------------------------------------------
  Executed: Create Coach
   - Trace: Coach Name = Coach-5
 ------------------------------------------------------------
  Executed: Create Course
   - Trace: Course Name = Course-3
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - Trace: Coach Name = Coach-5
   - Trace: Course Name = Course-3
 ------------------------------------------------------------
  Executed: Update Course Skills
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
The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment.  
