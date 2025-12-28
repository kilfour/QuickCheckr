# Guide
**Welcome to the QuickCheckr Guide.**  
This document walks through the core ideas of QuickCheckr,
from the basics of building a Checkr, to stateful testing, branching behaviour, pools, shrinking,
leading up to a real-world scenario.  
Each chapter builds on the previous one, introducing new concepts through executable examples.  
## Contents

- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [The Stash, Runs and executions][TheStashRunsAndExecutions]
  -  An example and an explanation of how QuickCheckr handles stateful testing.
- [Choosing the Next Move][ChoosingTheNextMove]
  -  A brief introduction to behavioural branching.
- [In Pools We Trust][InPoolsWeTrust]
  - An overview of pools and their usage.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.  

[CheckringThingsOut]: #checkr-ing-things-out

[TheStashRunsAndExecutions]: #the-stash-runs-and-executions

[ChoosingTheNextMove]: #choosing-the-next-move

[InPoolsWeTrust]: #in-pools-we-trust

[BoltingHorses]: #bolting-horses
## Checkr-ing Things Out
[Top][Guide] | [Next][TheStashRunsAndExecutions]  
  
A *Checkr* describes a sequence of steps that QuickCheckr will execute repeatedly.
Each run goes through the script from top to bottom: generating inputs, performing actions, checking expectations, and collecting diagnostics.
If any execution fails, QuickCheckr shrinks it to the smallest, easiest-to-understand failing case.  
### First Expectation
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:64:1
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
### Gathering Input
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:90:1
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:132:1
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
### Not Without a Trace
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:169:1
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
### Time to Act!
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:209:1
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

[Top][Guide] | [Next][TheStashRunsAndExecutions]  

[Guide]: #guide

[TheStashRunsAndExecutions]: #the-stash-runs-and-executions
## The Stash, Runs and Executions
[Top][Guide] | [Previous][CheckringThingsOut] | [Next][ChoosingTheNextMove]  
  
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
 Location:                Docs\A_Guide\B_TheStashRunsAndExecutions\TheStashRunsAndExecutions.cs:75:1
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
### Recap
QuickCheckr operates across *two distinct scopes*, and understanding them is essential when testing stateful systems:

**1. Run Scope**

A *run* is a full pass through your Checkr script.
Every run starts with a clean slate:

* All `Trackr.Stashed` values are reset.
* Pools (explained [later][InPoolsWeTrust]) start empty.
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
  

[Top][Guide] | [Previous][CheckringThingsOut] | [Next][ChoosingTheNextMove]  

[InPoolsWeTrust]: #in-pools-we-trust

[Guide]: #guide

[CheckringThingsOut]: #checkr-ing-things-out

[ChoosingTheNextMove]: #choosing-the-next-move
## Choosing the Next Move
[Top][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  
  
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
  
### Seeing Which Branches Were Chosen
Running the Checkr without shrinking yields a report similar to this:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:72:1
 Original failing run:    5 executions
 Seed:                    1509195528
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 61
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 13
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 61
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 20
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 98
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 4x
 ------------------------------------------------------------
```
This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.  
### Shrinking Branching Behaviour
And if we let the QuickCheckr shrinker do its work:  
```text
------------------------------------------------------------
 Test:                    RunTest_Shrunk
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:123:1
 Original failing run:    5 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1509195528
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 98
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 4x
 ------------------------------------------------------------
```
The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
  
### On One Condition
In many cases you would like to control the possible branches taken, based on some condition.  
For this you can use `Checkr.OneOfWhen`, which takes an enumerable of `Checkr.Option` elements.  

**Example:**  
```csharp
    from everything in Trackr.Stashed(() => new Everything())
    from operations in Checkr.OneOfWhen(
        Checkr.Option(() => everything.TheUniverse == null,
            from genesis in Checkr.Act("Create the universe",
                () => everything.TheUniverse = new Universe())
            select Case.Closed),
        Checkr.Option(() => everything.TheUniverse != null,
            from falsify in Checkr.Act("Falsify the answer",
                () => everything.TheUniverse!.Answer++)
            select Case.Closed)
    )
    from checkTheAnswer in Checkr.ExpectWhen("is 42",
        () => everything.TheUniverse != null,
        () => everything.TheUniverse!.Answer == 42)
    select Case.Closed;
```
**Results in:**  
```text
------------------------------------------------------------
 Test:                    RunTest_Conditional
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:182:1
 Original failing run:    2 executions
 Seed:                    356083796
 ------------------------------------------------------------
  Executed: Create the universe
 ------------------------------------------------------------
  Executed: Falsify the answer
 ================================
  !! Expectation Failed: is 42
 ================================
 Passed Expectations
 - is 42: 1x
 ------------------------------------------------------------
```
### Recap
`Checkr.OneOf` and `Checkr.OneOfWhen` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.  

[Top][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  

[Guide]: #guide

[TheStashRunsAndExecutions]: #the-stash-runs-and-executions

[InPoolsWeTrust]: #in-pools-we-trust
## In Pools We Trust
[Top][Guide] | [Previous][ChoosingTheNextMove] | [Next][BoltingHorses]  
  

**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, Pool operations (PoolWhen, PoolExpect, ToPool, ...) encapsulate "is this state valid?" so shrinking can replay safely.
  
### Creating a Pool
Pools are created for *a specific type* using:  
```csharp
Trackr.PoolFor<Contact>();
```
Which is a Pool for the following type:  
```csharp
public record Contact(string Name)
{
    public string Name { get; set; } = Name;
}
```
### Storing elements in the Pool
```csharp
from pool in Trackr.PoolFor<Contact>()                     // Create a Pool.
from name in Checkr.Input("Name", Fuzzr.String())          // Generate a random String.
from storeName in Trackr.ToPool("Name", new Contact(name)) // <= And in it goes.
select Case.Closed;
```
### Using random elements from a Pool
```csharp
Trackr.OneOfPool<Contact>("A Random Contact",
    contact => Checkr.Trace("Contact Name", () => contact.Value.Name));
```
As you can see, the second argument to this method, is a function that takes a wrapped value and returns a Checkr.  
In the same vein that the Pool collection itself is encapsulated, so are its elements.
This is done because you might want to remove the element from the Pool, or replace it, and the shrinker needs to be aware of these operations.  
Therefore, the operations are defined on the wrapper object.

*Remove:*  
```csharp
Trackr.OneOfPool<Contact>("A Random Contact to Remove",
    contact => contact.Remove());
```
*Replace:*  
```csharp
Trackr.OneOfPool<Contact>("A Random Contact to Replace",
    contact => contact.Replace(new Contact("Replaced by AI")));
```
#### Conditionally
Now, when picking a random element like this, there has to exist atleast one in the Pool, or else a QuickFuzzr `OneOfEmptyOptionsException` will be thrown.  
Which means, you almost always want to execute this conditionally,
and most likely it will be part of a random sequence of executions,
as you will need another execution to put things into the Pool.  
For this reason the following method returns a `Checkr.Option`, in order to make it fit neatly into a `OneOfWhen` Checkr.
  
```csharp
Trackr.PoolWhen<Contact>("A Random Contact", a => a.Count != 0,
    contact => Checkr.Trace("Contact Name", () => contact.Value.Name));
```
### Bringing it all back home
#### The Model
```csharp
public class Rolodex
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
    from contactBook in Trackr.Stashed(() => new Rolodex())
    // Create a Pool.
    from createContactPool in Trackr.PoolFor<Contact>()
        // Pick one of two operations.
    from operations in Checkr.OneOfWhen(
        // 1. Add a (Contact-) name to the Rolodex
        Trackr.PoolWhen<Contact>(a => a.Count < 10,
            from name in Checkr.Input("Name", NameFuzzr)
            from add in Checkr.Act("Add", () => contactBook.Add(name))
                // Store the Contact in the Pool.
            from storeName in Trackr.ToPool("Store", new Contact(name))
            select Case.Closed),
        // 2. Update a (Contact-) name in the Rolodex
        Trackr.PoolWhen<Contact>("Contact to update",
            a => a.Count != 0, // <= Can not pick from an empty selection.
            contact => // Pick a random Contact to update 
                from newName in Checkr.Input("New Name", NameFuzzr)
                from update in Checkr.Act("Update", () => contactBook.Update(contact.Value.Name, newName))
                    // Update the contact in the pool, keeping model and test data in sync.
                from updatePool in contact.Replace(new Contact(newName))
                select Case.Closed))
        // This expectation implicitely verifies that duplicates are allowed. 
    from count in Trackr.PoolExpect<Contact>("Count matches", a => a.Count == contactBook.Contacts.Count)
    select Case.Closed;
```
#### The Report
```text
------------------------------------------------------------
 Test:                    AllTogetherNow
 Location:                Docs\A_Guide\D_InPoolsWeTrust\InPoolsWeTrust.cs:151:1
 Original failing run:    10 executions
 Minimal failing case:    3 executions (after 8 shrinks)
 Seed:                    1444257086
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Store = Contact-5
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Store = Contact-6
   - Input: Name = "Paul Harrison"
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Contact to update = Contact-5
   - Input: New Name = "Paul Harrison"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 9x
 ------------------------------------------------------------
```

[Top][Guide] | [Previous][ChoosingTheNextMove] | [Next][BoltingHorses]  

[Guide]: #guide

[ChoosingTheNextMove]: #choosing-the-next-move

[BoltingHorses]: #bolting-horses
## Bolting Horses
[Top][Guide] | [Previous][InPoolsWeTrust]  
  
Up to this point we have seen a few examples of how to use QuickCheckr to test a system, but the bugs have been relatively trivial.
For this one, we're going to look at a real bug found in the wild, which is demonstrative for a whole category: *Invariants drifting over time*.

*So, here we go:*  
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
It then creates a series of actions, randomly choosing one, using `Checkr.OneOfWhen`,
out of those in the list for which it's predicate is fulfilled.  
Finally after each action we check the `AssignedCoachesMustBeSuitable` Invariant.  
```csharp
public static readonly CheckrOf<Case> TheCheckr =
    from coaches in Trackr.PoolFor<Coach>()  // Create a pool of coaches.
    from courses in Trackr.PoolFor<Course>() // Create a pool of courses.
    from operations in Checkr.OneOfWhen(
        // Create up to 10 coaches.
        Trackr.PoolWhen<Coach>(a => a.Count() < 10, CreateCoach),
        // Maybe update existing coaches. Pick a random coach from the pool.
        Trackr.PoolWhen("Coach to Update Skills", a => a.Count != 0, UpdateCoachSkills),
        // Create up to 5 courses.
        Trackr.PoolWhen<Course>(a => a.Count() < 5, CreateCourse),
        // Maybe update existing courses. Pick a random course from the pool.
        Trackr.PoolWhen("Course to Update Skills", a => a.Count != 0, UpdateCourseSkills),
        // Maybe assign a coach, if there exists a combination for which the predicate holds.
        Trackr.PoolWhen("Assign Coach to Course",
            (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
        // Ensure the system invariant is met at all times.
    from invariant in AssignedCoachesMustBeSuitable
    select Case.Closed;
```
**Create Coach Checkr:**  
In this one we create a coach with a *semi* random name, verify that the property `Name` is assigned and then add it to the coaches pool.  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from coach in Checkr.Act("Create Coach", () => new Coach(name))
    from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
    from toPool in Trackr.ToPool("Store Coach", coach) // Store this coach in the pool.
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
Now we retrieve a random coach from the pool and update its skills, verifying that the property `Skills` is set correctly.  
```csharp
private static readonly Func<PoolElement<Coach>, CheckrOf<Case>> UpdateCoachSkills =
    coach =>
        from skills in Checkr.Input("Coach Skills",
            Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => coach.Value.UpdateSkills(skills)) // Update the Coach.
        from checkSkills in Checkr.Expect("Coach Skills Set",
            () => coach.Value.Skills.Count
               == skills.Count() && coach.Value.Skills.All(skills.Contains))
        select Case.Closed;
```
**Create Course Checkr:**  
Same pattern as the *Create Coach Checkr*.  
```csharp
private static readonly CheckrOf<Case> CreateCourse =
    from name in Checkr.Input("Course Name",
        from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
    from course in Checkr.Act("Create Course", () => new Course(name))
    from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
    from toPool in Trackr.ToPool("Store Course", course) // Store this course in the pool.
    select Case.Closed;
```
**Update Course Skills Checkr:**  
And again, same pattern as the *Update Coach Skills Checkr*.  
```csharp
private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
    course =>
        from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Course Skills",
            () => course.Value.UpdateRequiredSkills(skills)) // Update the Course.
        from checkSkills in Checkr.Expect("Course Required Skills Set",
            () => course.Value.RequiredSkills.Count
               == skills.Count() && course.Value.RequiredSkills.All(skills.Contains))
        select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
And finally here's the system invariant.  
*Any coach assigned a course must have the required skills to teach that course*.  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from assignedCheck in Trackr.PoolExpectEachWhen<Coach>("Assigned Coach Must Be Suitable",
        coach => coach.AssignedCourses.Any(), // Check that all assigned coaches are suitable.
        coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\A_Guide\E_HorsesAgain\BoltingHorses.cs:75:1
 Original failing run:    19 executions
 Minimal failing case:    4 executions (after 21 shrinks)
 Seed:                    1473455315
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: Store Course = Course-2
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-2
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   -  Pool: Assign Coach to Course = ( Coach-2, Course-2 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-2
   - Input: Course Skills = [ _ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Course Name Set: 5x
 - Assigned Coach Must Be Suitable: 18x
 - Coach Name is set: 5x
 - Coach Skills Set: 3x
 - Coach is assigned: 5x
 - Course Required Skills Set: 1x
 ------------------------------------------------------------
```
The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment.  

[Top][Guide] | [Previous][InPoolsWeTrust]  

[Guide]: #guide

[InPoolsWeTrust]: #in-pools-we-trust
