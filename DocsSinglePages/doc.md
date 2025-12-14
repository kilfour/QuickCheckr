## Guide
**Welcome to the QuickCheckr Guide.**  
This document walks through the core ideas of QuickCheckr,
from the basics of building a Checkr, to stateful testing, branching behaviour, pools, shrinking,
leading up to a real-world scenario.  
Each chapter builds on the previous one, introducing new concepts through executable examples.  
### Contents

- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [The Stash, Runs and executions][TheStashRunsAndExecutions]
  -  An example and an explanation of how QuickCheckr handles stateful testing.
- [ChoosingTheNextMove]
  -  A brief introduction to behavioural branching.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.  

[CheckringThingsOut]: #checkr-ing-things-out

[TheStashRunsAndExecutions]: #the-stash-runs-and-executions

[ChoosingTheNextMove]: #choosing-the-next-move

[BoltingHorses]: #bolting-horses
### Checkr-ing Things Out
A *Checkr* describes a sequence of steps that QuickCheckr will execute repeatedly.
Each run goes through the script from top to bottom: generating inputs, performing actions, checking expectations, and collecting diagnostics.
If any execution fails, QuickCheckr shrinks it to the smallest, easiest-to-understand failing case.  
#### First Expectation
Let's start by having a look at the simplest possible Checkr:  
```csharp
public class MinimalCheckr
{
    [Fact]
    public void RunTest()
        => Checkr.Expect("Always true", () => true).Run();
}
```
Congratulations! You've just checked something out. The test passes and now we know that `true` is always `true`.

So what happens if try to do the opposite?  
```csharp
Checkr.Expect("Always true", () => false);
```
The test fails and QuickCheckr produces the following report:  
```text
------------------------------------------------------------
 Test:                    Run_ExpectAlwaysFalse
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:60:1
 Original failing run:    1 execution
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed
 ======================================
  !! Expectation Failed: Always true
 ======================================
```

The report shows a run summary containing :
* Test: Name of the test executing the Checkr.
* Location: Source code Location of the test executing the Checkr.
* Original failing run: Number of executions executed during the run.
* The minimal fail-case: In this toyest of examples it *only* specifies that an execution happened.
* Failure details: Why the run failed in the first place.
* Seed: The seed used to generate the failing run.  
  You can use this to reproduce the failing run reliably, by calling run with the seed passed in.  
```csharp
checkr.Run(1642083259);
```
#### Gathering Input
Let's add an input to the Checkr, and change our expectation:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from expect in Checkr.Expect("Input /= 42", () => input != 42)
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
check.Run(200.Runs())
```
Now the test fails consistently:  
```text
------------------------------------------------------------
 Test:                    AddingAnInput
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:86:1
 Original failing run:    1 execution
 Seed:                    127648209
 ------------------------------------------------------------
  Executed:
   - Input: Input = 42
 ======================================
  !! Expectation Failed: Input /= 42
 ======================================
```
As you can see the input is now included in the report *minimal fail-case*,
this is because QuickCheckr judged the input to be relevant to the failure.  
So, if we add another input to the Checkr, one which does not influence the failure:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from expect in Checkr.Expect("Input /= 42", () => input != 42)
    select Case.Closed;
```
The following happens:  
```text
------------------------------------------------------------
 Test:                    AddingAnotherInput
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:128:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    127648209
 ------------------------------------------------------------
  Executed:
   - Input: Input = 42
 ======================================
  !! Expectation Failed: Input /= 42
 ======================================
```
That's basically the same thing as before, because QuickCheckr does not report irrelevant inputs.  
#### Not Without a Trace
QuickCheckr's *shrinking* can sometimes seem magical. Not a problem in this simple test,
but when you are writing a more complex test, it is always helpful to see what's really going on.

QuickCheckr is built with transparency and discoverability as a first class concern:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from expect in Checkr.Expect("Input /= 42", () => input != 42)
    from trace in Checkr.Trace("Irrelevant Input", () => irrelevant)
    select Case.Closed;
```
By adding a `Checkr.Trace` to the `LINQ` chain, we get the following report:  
```text
------------------------------------------------------------
 Test:                    NotWithoutATrace
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:165:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    769641533
 ------------------------------------------------------------
  Executed:
   - Input: Input = 42
   - Trace: Irrelevant Input = 47
 ======================================
  !! Expectation Failed: Input /= 42
 ======================================
```
#### Time to Act!
Testing inputs is fun and all, but really, we want to test some kind of behaviour, right?  
```csharp
var check =
    from one in Checkr.Input("One", Fuzzr.Int())
    from two in Checkr.Input("Two", Fuzzr.Int())
    from three in Checkr.Input("Three", Fuzzr.Int())
    from sum in Checkr.Act("One plus Three", () => one + three)
    from expect in Checkr.Expect("Sum /= 42", () => sum != 42)
    select Case.Closed;
```
Still a trivial example, but it shows how you can use `Checkr.Act` to start mutating things.  
And here's the report:  
```text
------------------------------------------------------------
 Test:                    TimeToAct
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:204:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed: One plus Three
   - Input: One = 24
   - Input: Three = 18
 ====================================
  !! Expectation Failed: Sum /= 42
 ====================================
```
As demonstrated, the name of the executed action is included in the report, as well as the relevant inputs.  
### The Stash, Runs and Executions
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
    from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
    select Case.Closed;
```
The test will always pass, even if we increase the amount of runs to infinity,
because the test loses the state of the `BugHouse` between every run.  
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
    from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
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
 Location:                Docs\A_Guide\B_TheStashRunsAndExecutions\TheStashRunsAndExecutions.cs:63:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1854571642
 ------------------------------------------------------------
  Executed: BugHouse.DelayedDetonation (2 Times)
 ------------------------------------------------------------
  Executed: BugHouse.DelayedDetonation
   - Input: Int = 1
 ===========================================================================
  !! Exception Thrown: QuickPulse.Instruments.ComputerSaysNo: I'm sorry, Dave, I'm afraid I can't do that.
   at QuickPulse.Instruments.ComputerSays.No(String msg)
   at QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions.TheStashRunsAndExecutions.BugHouse.DelayedDetonation(Int32 a) in Docs\A_Guide\B_TheStashRunsAndExecutions\TheStashRunsAndExecutions.cs:line 26
   at QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions.TheStashRunsAndExecutions.<>c__DisplayClass8_2..cctor() in Docs\A_Guide\B_TheStashRunsAndExecutions\TheStashRunsAndExecutions.cs:line 54
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 71
 ===========================================================================
 Passed Expectations
 - Returns true: 2x
 ------------------------------------------------------------
```
The shrunk case makes it immediately clear why this Checkr needed multiple executions.  

QuickCheckr shows that the action BugHouse.DelayedDetonation was executed twice without issue,
and only on the third execution, where the relevant input `Int = 1` appears, does the exception occur.  

Because the BugHouse instance is stashed, its internal count persisted across executions, allowing the failure to surface.  

The shrinking process confirms that all three executions are required:
removing either of the first two would prevent the internal state from reaching the dangerous value.  

Finally, the report closes by listing which expectations passed during the original run,
highlighting that the behaviour was stable until exactly the offending execution.  
#### Recap
QuickCheckr operates across *two distinct scopes*, and understanding them is essential when testing stateful systems:

**1. Run Scope**

A *run* is a full pass through your Checkr script.
Every run starts with a clean slate:

* All `Trackr.Stashed` values are reset.
* Pools (`Trackr.Pool`, explained later) start empty.
* Counters, uniqueness constraints, and any internal memories tied to a run are cleared.

This guarantees that each run is **independent**, **repeatable**, and **reproducible** based on the seed.

**But inside a run**, QuickCheckr may execute your script multiple times.

### **2. Execution Scope**

An *execution* is one walk through the Checkr script *within* a single run.
Executions share:

* The **same stashed instances** (e.g. the `BugHouse`).
* The **same pools**, which accumulate objects over executions.
* The **effects** of all previous executions in the run.

Meanwhile, the following *are fresh each execution*:

* Every `Checkr.Input` value.
* Every `Checkr.Act`, `Checkr.Expect`, and `Checkr.Trace` is evaluated anew.
* The shrinking engine can re-run isolated executions to evaluate relevance.

This means:

* **Runs give you independence.**
* **Executions give you statefulness.**

And together, they let QuickCheckr explore both *pure input variation* and *multi-step behaviour*,
which is exactly what makes the BugHouse example catchable:
the failure only emerges when several executions act on the *same* object.

**!! Important !! :** The static factory classes follow the same scoping model:
- `Checkr.*` combinators are execution-scoped: they run once per execution and do not persist state.
- `Trackr.*` combinators are run-scoped: they hold state that persists across all executions within a run.  
### Choosing The Next Move
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
#### A Checkr With a Choice
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int())
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
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
  
#### Seeing Which Branches Were Chosen
Running the Checkr without shrinking yields a report similar to this:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:64:1
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
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 7x
 ------------------------------------------------------------
```
This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.  
#### Shrinking Branching Behaviour
And if we let the QuickCheckr shrinker do its work:  
```text
------------------------------------------------------------
 Test:                    RunTest_Shrunk
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:144:1
 Original failing run:    8 executions
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    1169776765
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 66
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
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
#### Recap
`Checkr.OneOf` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.
* Shrinking works naturally: QuickCheckr does **not** try unchosen branches. 
  It only minimizes the actual failing scenario.  
### Bolting Horses
Up to this point we have seen a few examples of how to use QuickCheckr to test a system, but the bugs have been relatively trivial.
For this one, we're going to look at a real bug found in the wild, which is demonstrative for a whole category: *Invariants drifting over time*.

The only concept we haven't covered yet, and that we need here, is:

**Pools**  
These let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  
In this example, we'll keep all created coaches and courses in pools,
and each operation will pick from those pools to mutate the system over time.

We are going to  use a simplified version of the [`HorsesForCourses`](https://github.com/kilfour/HorsesForCourses/blob/main/README.md) domain.  


The invariant we are checking is : 
- *Any coach assigned a course must have the required skills to teach that course*.

And here's the domain model:
  
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
        if (!coach.IsSuitableFor(this)) // this enforces the invariant
            throw new InvalidOperationException("Coach does not have the required skills");
        AssignedCoach = coach;
        coach.AssignCourse(this);
    }
}
```
Our Checkr is composed out of several parts:  


**The Composed Checkr:**  
This one is the *entry point* for the test.  
It creates two pools, one for coaches, and one for courses, which we are going to use to track the state of the system.  
It then creates a series of actions, randomly choosing one, using `Checkr.OneOfIf`,
out of those in the list for which it's predicate is fulfilled.  
Finally after each action we check the `AssignedCoachesMustBeSuitable` Invariant.  
```csharp
public static readonly CheckrOf<Case> TheCheckr =
    from coaches in Trackr.PoolFor<Coach>()         // Create a pool of coaches.
    from courses in Trackr.PoolFor<Course>()        // Create a pool of courses.
    from operations in Checkr.OneOfIf(
        (() => coaches.Count() < 10, CreateCoach),  // Create up to 10 coaches.
        (() => coaches.Any(), UpdateCoachSkills),   // Maybe update existing coaches.
        (() => courses.Count() < 5, CreateCourse),  // Create up to 5 courses.
        (() => courses.Any(), UpdateCourseSkills),  // Maybe update existing courses.
        // Maybe assign a coach to a course.
        (() => coaches.Any() && courses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
    from invariant in AssignedCoachesMustBeSuitable // Ensure the system invariant is met at all times.
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
    from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
    from toPool in Trackr.Pool("Store Coach", coach) // Store this coach in the pool.
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
Now we retrieve a random coach from the pool and update its skills, verifying that the property `Skills` is set correctly.  
```csharp
private static readonly CheckrOf<Case> UpdateCoachSkills =
    from coach in Trackr.OneOfPool<Coach>("Coach to update") // Pick a random coach from the pool.
    from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from valid in Checkr.When(() => coach is not null,
        from trace in Checkr.Trace("Coach Name", () => coach.Name)
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => coach.UpdateSkills(skills))                // Update it.
        from checkSkills in Checkr.Expect("Coach Skills Set",
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
    from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
    from toPool in Trackr.Pool("Store Course", course) // Store this course in the pool.
    select Case.Closed;
```
**Update Course Skills Checkr:**  
And again, same pattern as the *Update Coach Skills Checkr*.  
```csharp
private static readonly CheckrOf<Case> UpdateCourseSkills =
    from course in Trackr.OneOfPool<Course>("Course to update") // Pick a random course from the pool.
    from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from courseNotNull in Checkr.When(() => course is not null,
        from traceCourse in Checkr.Trace("Course Name", () => course.Name)
        from updateSkills in Checkr.Act("Update Course Skills",
            () => course.UpdateRequiredSkills(skills))          // Update it.
        from checkSkills in Checkr.Expect("Course Required Skills Set",
            () => course.RequiredSkills.Count == skills.Count() && course.RequiredSkills.All(skills.Contains))
        select Case.Closed)
    select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
And finally here's the system invariant.  
*Any coach assigned a course must have the required skills to teach that course*.  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from coaches in Trackr.Pool<Coach>() // Retrieve all coaches from the pool.
    from assignedCheck in Checkr.ExpectWhen("Assigned Coaches Must Be Suitable",
        () => coaches.Any(coach => coach.AssignedCourses.Any()),
        () => coaches.All( // Check that all assigned coaches are suitable.
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course))))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\A_Guide\E_HorsesAgain\BoltingHorses.cs:78:1
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
 ============================================================
  !! Expectation Failed: Assigned Coaches Must Be Suitable
 ============================================================
 Passed Expectations
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
## Putting a Case on the Board
### Test Framework Integration
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  
### Running a Checkr
This is done by calling `Run` on the Checkr instance.  
Several overloads are available:  
- Run the Checkr once with a single execution.  
```csharp
checkr.Run();
```
- Run the Checkr multiple times, each consisting of a single execution.  
```csharp
checkr.Run(5.Runs());
```
- Run the Checkr once but performs multiple executions within that run.  
```csharp
checkr.Run(5.ExecutionsPerRun());
```
- Run the Checkr with the specified number of runs and executions per run.  
```csharp
checkr.Run(3.Runs(), 5.ExecutionsPerRun());
```
- Run the Checkr with the specified seed and one execution.  
```csharp
checkr.Run(424242);
```
- Run the Checkr with the specified seed and the specified number of executions.  
```csharp
checkr.Run(424242, 5.ExecutionsPerRun());
```
All of the methods mentioned above can take an optional parameter `Func<CheckrConfig, CheckrConfig> configure`
which allows for controlling specific aspects of the Checkr run. We'll get into that later.  
### Configuration
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about  
**Default Configuration:**  
```csharp
{
    ShrinkMode = ShrinkMode.Default,
    DeliberationPolicy = null,
    FileAs = string.Empty,
    Custodian = Custodian.Default,
    Investigations = [],
    Editor =
        new ManOfWealthAndTaste()
            .ToUseNonLinearTime()
            .ToInline<List<int>>(),
    WithTraces = true
}
```
#### ShrinkMode
Controls which shrinking strategies are active.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from summer in Trackr.Stashed(() => new Summer())
    from input in Checkr.Input("Input", Fuzzr.Int(1, 25))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
    from add in Checkr.Act("Add", () => summer.Add(input))
    from expect in Checkr.Expect("Total should not be 42",
        () => summer.Total != 42)
    select Case.Closed;
```
**`ShrinkMode.None`**  
```csharp
private static CheckrConfig ConfigureNone(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
No shrinking. QuickCheckr reports the original failing run.  
```text
------------------------------------------------------------
 Test:                    RunTestNone
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\A_ShrinkMode\A_ShrinkMode.cs:46:1
 Original failing run:    4 executions
 Seed:                    1510302884
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 11
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 17
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 2
   - Input: Irrelevant = 8
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 12
   - Input: Irrelevant = 21
 =================================================
  !! Expectation Failed: Total should not be 42
 =================================================
 Passed Expectations
 - Total should not be 42: 3x
 ------------------------------------------------------------
```

**`ShrinkMode.Executions`**  
```csharp
private static CheckrConfig ConfigureExecutions(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
Trims away irrelevant executions.  
```text
------------------------------------------------------------
 Test:                    RunTestExecutions
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\A_ShrinkMode\A_ShrinkMode.cs:100:1
 Original failing run:    4 executions
 Seed:                    1510302884
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 11
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 17
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 2
   - Input: Irrelevant = 8
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 12
   - Input: Irrelevant = 21
 =================================================
  !! Expectation Failed: Total should not be 42
 =================================================
 Passed Expectations
 - Total should not be 42: 3x
 ------------------------------------------------------------
```

**`ShrinkMode.Inputs`**  
```csharp
private static CheckrConfig ConfigureInputs(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
Trims inputs to find a minimal failing case.  
```text
------------------------------------------------------------
 Test:                    RunTestInputs
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\A_ShrinkMode\A_ShrinkMode.cs:155:1
 Original failing run:    4 executions
 Minimal failing case:    4 executions (after 4 shrinks)
 Seed:                    1510302884
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 11
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 17
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 2
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 12
 =================================================
  !! Expectation Failed: Total should not be 42
 =================================================
 Passed Expectations
 - Total should not be 42: 3x
 ------------------------------------------------------------
```

**`ShrinkMode.Reduction`**  
```csharp
private static CheckrConfig ConfigureReduction(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Reduction };
```
Applies configured reducers for fine-grained shrinking.  
**Note:** This is an experimental feature, and will be expanded in the near future. As of now reporting with only this mode active is not supported.  
\n**`ShrinkMode.Default`**  

The default settings includes all possible ShrinkModes.  
#### Filing Evidence
QuickCheckr can be configured to persist the results of a failed test run.  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { FileAs = "MyQuickCheckrTest" };
```
Above configuration will write the resulting report of a failing run to a `report.qr` file in a folder relative to the solution root:  
```csharp
.quickcheckr\MyQuickCheckrTest\report.qr
```
In addition a sub folder is created containing the evidence vault files:  
```csharp
.quickcheckr\MyQuickCheckrTest\vault\evidence.qv
```
```csharp
.quickcheckr\MyQuickCheckrTest\vault\evidence-log.ql
```

- `evidence.qv` contains a list of all failing seeds, whenever a test fails, the seed is added to this file.
- `evidence-log.ql` is an append only log of all evidence vault actions.

This *evidence* can later be used to potentially solve *Cold Cases*.  
#### Investigations
These tests show the initial version of QuickCheckrs logging configuration.  
## On Shrinking
### Philosophy

QuickCheckr keeps shrinking simple and predictable.  
It never tries to *outsmart* your domain with clever global algorithms.  
Shrinking is purely local and irrelevance-based: if removing a value (or execution) still makes the property fail,
QuickCheckr marks it as *irrelevant*.  
This avoids surprising or *magical* shrink results.  
### Execution Shrinking
This is the first step in QuickCheckr shrinking.  
The following simple example fails on the third execution.  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
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
 Location:                Docs\H_OnShrinking\A_ExecutionShrinking\ExecutionShrinking.cs:53:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
### Input Shrinking
This is the next step in QuickCheckr shrinking and the most complex one.  
QuickCheckr shrinking tries to remove inputs, one by one, to find a minimal failing case.  
The following simple example has one input but always fails.  
```csharp
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from expect in Checkr.Expect("Always Fails", () => false)
    select Case.Closed;
```
Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after *Input Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:37:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    165641598
 ------------------------------------------------------------
  Executed
 =======================================
  !! Expectation Failed: Always Fails
 =======================================
```
Another example, slightly trickier.  
```csharp
    from one in Checkr.Input("Input One", Fuzzr.Int())
    from two in Checkr.Input("Input Two", Fuzzr.Int())
    from three in Checkr.Input("Input Three", Fuzzr.Int())
    from result in Checkr.Act("Result", () => one + three)
    from expect in Checkr.Expect("Should not be 42", () => result != 42)
    select Case.Closed;
```
As you can see from the test definition, two inputs are involved in a possible failure.

QuickCheckr correctly identifies the relevant inputs.

Result:
  
```text
------------------------------------------------------------
 Test:                    Example_Two
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:78:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    231122398
 ------------------------------------------------------------
  Executed: Result
   - Input: Input One = 12
   - Input: Input Three = 30
 ===========================================
  !! Expectation Failed: Should not be 42
 ===========================================
```
#### Collections
##### Removing Irrelevant Items
When shrinking collections, QuickCheckr does not attempt to rewrite or regenerate the list.
Instead, it removes items one by one, replaying the run each time to test whether the removed element was relevant for the failure.

This means that if only some elements in a list contribute to the failing condition, QuickCheckr will aggressively trim the rest away. The result is a minimal list containing only the values that still trigger the failure, even if the original list was large or noisy.

Consider a test that forbids the value `7`:  
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
    from expect in Checkr.Expect("List does not contain 7", () => !listInput.Contains(7))
    from trace in Checkr.Trace("List Input", () => listInput)
    select Case.Closed;
```
Even if the original list was long and varied, QuickCheckr isolates the single relevant element:  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\B_Collections\TheExamples\A_RemovingIrrelevantItems.cs:26:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1833095082
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 7 ]
   - Trace: List Input = [ 69, 49, 90, 36, 16, 71, 95, 7, 84 ]
 ==================================================
  !! Expectation Failed: List does not contain 7
 ==================================================
```
##### Removing Irrelevant Collection
Ofcourse a collection itself is just an input like another, so if it, as a whole,
is irrelevant for the failure, QuickCheckr will remove it.  
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
    from expect in Checkr.Expect("Always fails", () => false)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\B_Collections\TheExamples\B_RemovingIrrelevantCollection.cs:22:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2014608033
 ------------------------------------------------------------
  Executed
 =======================================
  !! Expectation Failed: Always fails
 =======================================
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
    from expect in Checkr.Expect("Length <= 2", () => list.Count() <= 2)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\B_Collections\TheExamples\C_IrrelevantElementButListLengthIsNot.cs:30:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1393576429
 ------------------------------------------------------------
  Executed:
   - Input: List = [ _, _, _ ]
   - Trace: Original List = [ 12, 45, 56, 18, 31 ]
 ======================================
  !! Expectation Failed: Length <= 2
 ======================================
```
### Deliberation Shrinking
Sometimes the shrinker comes up with a *local* minimal fail-case, not *the* minimal fail-case.  
Deliberation Shrinking allows us to instruct the Shrinker to explore the input further in a directed fasion.  
It is an opt-in feature, activated through the Configuration object.

Consider the following model:  
```csharp
public class Account
{
    public int Balance { get; private set; } = 0;
    public void Deposit(int amount)
    {
        Balance += amount;
        ApplyFee();
    }
    public void WithDraw(int amount)
    {
        if (amount > Balance) return;
        Balance -= amount;
        ApplyFee();
    }
    private void ApplyFee() => Balance -= 1;
}
```
It's a simple bank account with a no-overdraft invariant and a fee applied on each transaction.  
We can verify this with the following Checkr:  
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int(1, 10))
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
Now if we run this we might get something like this:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:66:1
 Original failing run:    3 executions
 Seed:                    1920827676
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 8
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 1
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 5
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 2x
 ------------------------------------------------------------
```
As you can see, this is a *local* minimal fail-case, but a smaller one exists.  

Let's configure a *DeliberationPolicy*, looking for a fail-case with less executions:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumExecutions };
```
Running this test now, with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.  
```text
------------------------------------------------------------
 Test:                    RunTest_Deliberated
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:111:1
 Original failing run:    2 executions
 Seed:                    24559414
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 3
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 2
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 1x
 ------------------------------------------------------------
```
#### An Eager Shrinker
Under some rare circumstances, the opposite can also occur, and the Shrinker gets *greedy*.  
Currently the only identified cases where this happens is if more than one input triggers the same failure.  
```csharp
    from input in Checkr.Input("Input", Fuzzr.Int(0, 5).Many(3))
    from trace in Checkr.Trace("List", () => input)
    from expect in Checkr.ExpectWhen("Does not contain 1",
        () => input.Count() > 2,
        () => !input.Contains(1))
    select Case.Closed;
```
Now when the list contains multiple elements `1`, we might get:  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedy
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:140:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    227109555
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, _ ]
   - Trace: List = [ 1, 2, 1 ]
 =============================================
  !! Expectation Failed: Does not contain 1
 =============================================
```
It seems QuickCheckr thinks that the minimal fail-case is a list with atleast 3 elements (*correct*),
but the element values are considered irrelevant (*wrong*).  
For this rare case we can also add a DeliberationPolicy, allowing QuickFuzzr to surface a more explanatory fail-case:  
```csharp
private static CheckrConfig ConfigureShrinkCount(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumShrinkCount };
```
**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyFixed
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:181:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1291119307
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ 1, _, _ ]
   - Trace: List = [ 1, 0, 3 ]
 =============================================
  !! Expectation Failed: Does not contain 1
 =============================================
```
A similar thing can happen when shrinking objects, and the solution is exactly the same  
```csharp
    from obj in Checkr.Input("BaggyInters",
        from cfg in Configr.Primitive(Fuzzr.Int(1, 5))
        from result in Fuzzr.One<BaggyInters>()
        select result)
    from trace in Checkr.Trace("Original", () => obj)
    from expect in Checkr.Expect("obj.IntOne == 1 || obj.IntThree == 1",
        () => obj.IntOne != 1 && obj.IntThree != 1)
    select Case.Closed;
```
**With Deliberation Policty:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyObjects
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:204:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1480133597
 ------------------------------------------------------------
  Executed:
   - Input: BaggyInters = { IntOne: 1 }
   - Trace: Original = { IntOne: 1, IntTwo: 3, IntThree: 2 }
 ===============================================================
  !! Expectation Failed: obj.IntOne == 1 || obj.IntThree == 1
 ===============================================================
```
## Cook Book
The QuickCheckr Cookbook is a collection of examples showing common patterns, 
testing techniques, and pitfalls you can check using QuickCheckr.  
### Verifying List Reversal (Double-Reverse Law)
**Demonstrates:**
- Custom Fuzzrs
- Relevant input shrinking
- Double-application property


I guess this one is very much tradition in PBT world, and who am I to argue with tradition, ... so here we go.

**The (buggy) implementation:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
        => list.Where(a => a != 42).Concat(list.Where(a => a == 42));
}
```
**The Fuzzr:**  
```csharp
var listFuzzr =
    from listLength in Fuzzr.Int(0, 10)
    from enumerable in Fuzzr.Int().Many(listLength)
    select enumerable.ToList();
```
**The Checkr:**  
```csharp
    from input in Checkr.Input("Input", ListFuzzr())
    from result in Checkr.Act("Result",
        () => TheReverser.Reverses(TheReverser.Reverses(input)))
    from trace in Checkr.Trace("Input ", () => input)
    from traceResult in Checkr.Trace("Result", () => result)
    from expect in Checkr.Expect(
        "Input == Result", () => input.SequenceEqual(result))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\T_Cookbook\A_ListReverse\ListReverse.cs:46:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    498116108
 ------------------------------------------------------------
  Executed: Result
   - Input: Input = [ _, 42 ]
   - Trace: Input  = [ 62, 13, 42, 71, 81, 27, 62, 28, 85 ]
   - Trace: Result = [ 42, 62, 13, 71, 81, 27, 62, 28, 85 ]
 ==========================================
  !! Expectation Failed: Input == Result
 ==========================================
```
### Testing Aggregate Behaviour (Trackr.Verify)
**Demonstrates:**
- Accumulated-state testing
- `Trackr.Verify`
- `Trackr.StopWhenProven`

While not strictly speaking a property-test, as there is no shrinking involved,
this kind of test is nevertheless very useful.  
So much so that QuickCheckr provides a dedicated tool for it. 

We're testing a simple implementation of a die:
  
```csharp
public class DieSix
{
    public int Roll() => new Random().Next(1, 6);
}
```
And we want to ensure that it rolls the numbers 1-6.  For which use the following Checkr:  
```csharp
    from die in Trackr.Stashed(() => new DieSix())
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
    from verify in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
`Trackr.Verify` is the tool you reach for when the correctness of a test depends on the accumulated state at the end of a run,
rather than the outcome of any single execution.
In this Loaded Die example, each roll is recorded in a stashed HashSet<int>, and the *verify* is evaluated only after the final execution has completed.
This makes it ideal for tests where you want to confirm that a whole series of operations, as a group, produced the expected overall behaviour.

**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\T_Cookbook\L_TheLoadedDie\TheLoadedDie.cs:71:1
 Original failing run:    100 executions
 Seed:                    80807098
 ------------------------------------------------------------
  Executed: Roll (100 Times)
 =================================================
  !!  The Verifier Disagrees: Rolls: Contains 6
 =================================================
```
Well, you probably saw this coming, but the die is bugged. `Random.Next` is upper-bound exclusive, so six is never rolled.  
A common mistake, and the verifier pointed it out in above report.

*Sidenote:* `Trackr.Verify`never triggers shrinking, because there is no meaningful *minimal failing case*,
either the final state satisfies all verifications or it doesn't.  
#### `Trackr.StopWhenProven`
```csharp
public class DieSixFixed
{
    public int Roll() => new Random().Next(1, 7);
}
```
Ok, we fixed the bug, so now our Checkr passes.
But what if we get lucky and we satisfy the verify after the first six rolls ?
Bit of a waste to continue running the Checkr, no ?  

Let's use `Trackr.StopWhenProven`:  
```csharp
    from die in Trackr.Stashed(() => new DieSixFixed())
    from cnt in Checkr.Perform(() => ++cell.Value)
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
    from proven in Trackr.StopWhenProven("All sides rolled",
        () => observer.Order().SequenceEqual([1, 2, 3, 4, 5, 6])) // <=
    from verify in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
Whenever the predicate passed into `Trackr.StopWhenProven` returns true,
the Checkr is considered proven and it concludes immediately, ... no more executions.  
