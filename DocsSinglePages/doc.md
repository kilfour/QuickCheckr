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
### Checkr-ing Things Out
[Guide][Guide] | [Next][TheStashRunsAndExecutions]  
  
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:169:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    769641533
 ------------------------------------------------------------
  Executed:
   - Input: Input            = 42
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
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:209:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed: One plus Three
   - Input: One   = 24
   - Input: Three = 18
 ====================================
  !! Expectation Failed: Sum /= 42
 ====================================
```
As demonstrated, the name of the executed action is included in the report, as well as the relevant inputs.  

[Guide][Guide] | [Next][TheStashRunsAndExecutions]  

[Guide]: #guide

[TheStashRunsAndExecutions]: #the-stash-runs-and-executions
### The Stash, Runs and Executions
[Guide][Guide] | [Previous][CheckringThingsOut] | [Next][ChoosingTheNextMove]  
  
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
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: BugHouse.DelayedDetonation
   - Input: Int = 1
 ===========================================================================
  !! ComputerSaysNo: I'm sorry, Dave, I'm afraid I can't do that.
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
  

[Guide][Guide] | [Previous][CheckringThingsOut] | [Next][ChoosingTheNextMove]  

[InPoolsWeTrust]: #in-pools-we-trust

[Guide]: #guide

[CheckringThingsOut]: #checkr-ing-things-out

[ChoosingTheNextMove]: #choosing-the-next-move
### Choosing the Next Move
[Guide][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  
  
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
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:72:1
 Original failing run:    2 executions
 Seed:                    1684147193
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 17
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 87
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 1x
 ------------------------------------------------------------
```
This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.  
#### Shrinking Branching Behaviour
And if we let the QuickCheckr shrinker do its work:  
```text
------------------------------------------------------------
 Test:                    RunTest_Shrunk
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:111:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1684147193
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 87
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 1x
 ------------------------------------------------------------
```
The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
  
#### On One Condition
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
 Location:                Docs\A_Guide\C_ChoosingTheNextMove\ChoosingTheNextMove.cs:169:1
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
#### Recap
`Checkr.OneOf` and `Checkr.OneOfWhen` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.  

[Guide][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  

[Guide]: #guide

[TheStashRunsAndExecutions]: #the-stash-runs-and-executions

[InPoolsWeTrust]: #in-pools-we-trust
### In Pools We Trust
[Guide][Guide] | [Previous][ChoosingTheNextMove] | [Next][BoltingHorses]  
  

**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

Pools exist to model systems where the set of things you can act upon change over time.  

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, Pool operations (PoolWhen, PoolExpect, ToPool, ...) encapsulate "is this state valid?" so shrinking can replay safely.
  
#### Creating a Pool
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
#### Storing elements in the Pool
```csharp
from pool in Trackr.PoolFor<Contact>()                     // Create a Pool.
from name in Checkr.Input("Name", Fuzzr.String())          // Generate a random String.
from storeName in Trackr.ToPool("Name", new Contact(name)) // <= And in it goes.
select Case.Closed;
```
#### Using random elements from a Pool
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
##### Conditionally
Now, when picking a random element like this, there has to exist atleast one in the Pool, or else a QuickFuzzr `OneOfEmptyOptionsException` will be thrown.  
Which means, you almost always want to execute this conditionally,
and most likely it will be part of a random sequence of executions,
as you will need another execution to put things into the Pool.  
For this reason the following method returns a `Checkr.Option`, in order to make it fit neatly into a `OneOfWhen` Checkr.
  
```csharp
Trackr.PoolWhen<Contact>("A Random Contact", a => a.Count != 0,
    contact => Checkr.Trace("Contact Name", () => contact.Value.Name));
```
#### Bringing it all back home
##### The Model
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
##### The Checkr
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
        // This expectation implicitely verifies that duplicates are not introduced. 
    from count in Trackr.PoolExpect<Contact>("Count matches", a => a.Count == contactBook.Contacts.Count)
    select Case.Closed;
```
##### The Report
```text
------------------------------------------------------------
 Test:                    AllTogetherNow
 Location:                Docs\A_Guide\D_InPoolsWeTrust\InPoolsWeTrust.cs:153:1
 Original failing run:    10 executions
 Minimal failing case:    3 executions (after 8 shrinks)
 Seed:                    1444257086
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Store = Contact-5
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Store = Contact-6
   - Input: Name  = "Paul Harrison"
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Contact to update = Contact-5
   - Input: New Name          = "Paul Harrison"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 9x
 ------------------------------------------------------------
```

[Guide][Guide] | [Previous][ChoosingTheNextMove] | [Next][BoltingHorses]  

[Guide]: #guide

[ChoosingTheNextMove]: #choosing-the-next-move

[BoltingHorses]: #bolting-horses
### Bolting Horses
[Guide][Guide] | [Previous][InPoolsWeTrust]  
  
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
        Trackr.PoolWhen<Coach>(a => a.Count < 10, CreateCoach),
        // Maybe update existing coaches. Pick a random coach from the pool.
        Trackr.PoolWhen("Coach to Update Skills", a => a.Count != 0, UpdateCoachSkills),
        // Create up to 5 courses.
        Trackr.PoolWhen<Course>(a => a.Count < 5, CreateCourse),
        // Maybe update existing courses. Pick a random course from the pool.
        Trackr.PoolWhen("Course to Update Skills", a => a.Count != 0, UpdateCourseSkills),
        // Maybe assign a coach, if there exists a combination for which the predicate holds.
        Trackr.PoolWhen("(Coach, Course) to Assign",
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
            Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(1, 3))
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
        from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(1, 3))
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
 Location:                Docs\A_Guide\E_HorsesAgain\BoltingHorses.cs:81:1
 Original failing run:    13 executions
 Minimal failing case:    4 executions (after 14 shrinks)
 Seed:                    1473455315
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: Store Course = Course-1
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-1
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   -  Pool: (Coach, Course) to Assign = ( Coach-1, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ _ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Course Name Set: 4x
 - Coach Name is set: 4x
 - Coach Skills Set: 2x
 - Coach is assigned: 2x
 - Assigned Coach Must Be Suitable: 3x
 - Course Required Skills Set: 1x
 ------------------------------------------------------------
```
The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment.

**Note:** The astute reader will have noticed there exists more than one way in which the invariant can be violated.  
Let me redirect said readers to [Gathering Evidence][GatheringEvidence].  

[Guide][Guide] | [Previous][InPoolsWeTrust]  

[Guide]: #guide

[InPoolsWeTrust]: #in-pools-we-trust

[GatheringEvidence]: #gathering-evidence
## Putting a Case on the Board
This chapter takes you through the different options on how to run, configure and rerun Checkrs.
It also explains the *Evidence Vault* system and how to gather evidence.
  
### Contents

- [Test Framework Integration][TestFrameworkIntegration]
- [Running a Checkr][RunningACheckr]
- [Configuration][Configuration]
- [The Evidence Vault][TheEvidenceVault]
- [Gathering Evidence][GatheringEvidence]
  

[TestFrameworkIntegration]: #test-framework-integration

[RunningACheckr]: #running-a-checkr

[Configuration]: #configuration

[TheEvidenceVault]: #the-evidence-vault

[GatheringEvidence]: #gathering-evidence
### Test Framework Integration
[The Board][PuttingACaseOnTheBoard] | [Next][RunningACheckr]  
  
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[RunningACheckr]: #running-a-checkr
### Running a Checkr
[The Board][PuttingACaseOnTheBoard] | [Previous][TestFrameworkIntegration] | [Next][Configuration]  
  
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
which allows for controlling specific aspects of the Checkr run.  

[The Board][PuttingACaseOnTheBoard] | [Previous][TestFrameworkIntegration] | [Next][Configuration]  

[Configuration]: #configuration

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[TestFrameworkIntegration]: #test-framework-integration

[Configuration]: #configuration
### Configuration
[The Board][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]  
  
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about.

All configuration options live in the `QuickCheckr.Protocol` namespace.  

**Default Configuration:**  
```csharp
{
    FileAs = string.Empty,
    Custodian = Custodian.Default,
    ShrinkMode = ShrinkMode.Default,
    ReportMode = ReportMode.Default,
    DeliberationPolicy = null,
    InternalAffairs = []
}
```
#### Filing Evidence
QuickCheckr can be configured to persist the results of a test run.  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { FileAs = "MyQuickCheckrTest" };
```
Above configuration will write the resulting report of a run to a `report.qr` file in a folder relative to the solution root:  
```csharp
.quickcheckr\MyQuickCheckrTest\report.qr
```
In case a test passes, a *summary report* is provided.  
```text
2 Runs
 Passed Expectations
 - Always Passes: 10x
 ------------------------------------------------------------
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
#### The Custodian
`TheCustodian` is in charge of storing and retrieving QuickCheckr artifacts.  
As we saw in 'Filing Evidence', QuickCheckr stores reports and failing seeds on disk.  
This is the default behaviour, but by configuring the `Custodian` we can change this behaviour.

Currently only one extra `Custodian` is provided: `Custodian.Local()`.  
This one stores everything in a '.quickcheckr' directory in the same directory as the test file that created it.
  
##### Custom Custodians

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  
You can also supply your own implementation:  
```csharp
public class InMemoryCustodian
{
    public Dictionary<string, InMemoryRecorder> Data = [];
    public List<string> StartingEvidence = [];
    public Custodian Build() =>
        new()
        {
            Recorder = fn =>
            {
                if (!Data.ContainsKey(fn))
                    Data[fn] = new InMemoryRecorder();
                return Data[fn];
            },
            Retriever = fn => StartingEvidence
        };
}

public class InMemoryRecorder : IEvidenceRecorder
{
    public bool Cleared;
    public List<string> Values = [];
    public void Append(string evidence) => Values.Add(evidence);
    public void Clear() { Cleared = true; Values.Clear(); }
}
```
This one stores evidence and reports in memory, and is used in the tests to avoid touching the filesystem  
#### Deliberation Policy
Sometimes the shrinker comes up with a *local* minimal fail-case, not *the* minimal fail-case.  
Configuring a *DeliberationPolicy* allows us to instruct the Shrinker to explore the input further in a directed fasion.  

In short: Deliberation controls how agressive the shrinker keeps searching after it has found a *local* minimal fail-case.

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
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\F_DeliberationPolicy\DeliberationPolicy.cs:66:1
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

Let's add a *DeliberationPolicy*, looking for a fail-case with less executions:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumExecutions };
```
Running this test now, with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.  
```text
------------------------------------------------------------
 Test:                    RunTest_Deliberated
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\F_DeliberationPolicy\DeliberationPolicyTests.cs:113:1
 Original failing run:    7 executions
 Minimal failing case:    2 executions (after 5 shrinks)
 Seed:                    136908528
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
 - No Overdraft: 6x
 ------------------------------------------------------------
```
The following predefined policies are available  
```csharp
- DeliberationPolicy.MinimumExecutions
- DeliberationPolicy.MinimumShrinkCount
- DeliberationPolicy.MaximumShrinkCount
- DeliberationPolicy.MinimumWarnings
- DeliberationPolicy.MaximumExecutionsShrunk
- DeliberationPolicy.MaximumDistinctPassedExpectations
```

See the documentation for [*Deliberation Shrinking*][DeliberationShrinking] for more examples and usage of these policies.

It is also possible to define a custom policy, but this is an ...  

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  

[DeliberationShrinking]: #deliberation-shrinking
#### ReportMode
You can use this to customize report formatting somewhat.  
I personally almost always disable the *Labels* ('Input:', 'Pool:', etc...), but your milage may vary.  

It is also useful to toggle traces on or off, for instance.  
```csharp
private static CheckrConfig DisableTraces(CheckrConfig config) =>
    config with { ReportMode = config.ReportMode & ~ReportMode.Trace };
```
A full reference to the available options can be found [here][A_ReportModeReference].  
#### ShrinkMode
Controls which shrinking strategies are active.  

This is an option you will likely never need.  
It's main use is to investigate what happened during shrinking.  
As such, disabling shrinking entirely is probably the most useful step.  
```csharp
private static CheckrConfig DisableShrinking(CheckrConfig config) =>
    config with { ShrinkMode = ShrinkMode.None };
```
A full reference to the available options can be found [here][A_ShrinkModeReference].  
#### Hint
If you do not like the defaults,
it is advised to simply wrap them into your own `CheckrOf` extension method(s):  
  
**Fast Deliberation:**  
```csharp
public static CaseFile Deliberate(
    this CheckrOf<Case> checkr,
    Func<CaseFile, CaseFile, CaseFile>? policy = null)
    => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
        cfg => cfg with { DeliberationPolicy = policy ?? DeliberationPolicy.MinimumExecutions });
```
**Auto File:**  
```csharp
public static CaseFile FileThis(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
         cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
```
**Investigating:**  
```csharp
public static void Investigate(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr.GatherEvidence(100.Investigations(), 10.Runs(), 20.ExecutionsPerRun(),
        cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
```
You get the idea.  
#### Internal Affairs
The *clue* is in the name. This is used to debug QuickCheckr itself when things go awry, and as such it is an ...  

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  
It might evolve into something that is *open to the public*, or it might get removed entirely, ... not sure.  

[The Board][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]  

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[RunningACheckr]: #running-a-checkr

[TheEvidenceVault]: #the-evidence-vault

[A_ReportModeReference]: #reportmode-reference

[A_ShrinkModeReference]: #shrinkmode-reference
##### ReportMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
    from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
    from trace in Checkr.Trace("Irrelevant", () => irrelevant)
    from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
    select Case.Closed;
```
###### ReportMode.Default
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Default };
```
Includes all possible ReportModes.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeDefault.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter    = 2
   - Input: Null Thing = null
   - Trace: Irrelevant = 73
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! QuickPulse.Instruments.ComputerSaysNo: Only one is available
   at QuickPulse.Instruments.ComputerSays.No[T](String msg)
   at QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference.A_ReportModeReference.<>c__DisplayClass1_1.<GetCheckr>b__9() in C:\Code\QuickCheckr\QuickCheckr.Tests\Docs\B_PuttingACaseOnTheBoard\C_Configuration\J_ReportMode\Reference\A_ReportModeReference.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
###### ReportMode.None
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.None };
```
No traces, no warnings, no labels, no stacktrace.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeNone.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
###### ReportMode.Trace
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Trace };
```
Only traces.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeTrace.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - Irrelevant = 73
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
###### ReportMode.Warning
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Warning };
```
Only warnings.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeWarning.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
###### ReportMode.Labels
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Labels };
```
Only labels.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeLabels.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter    = 2
   - Input: Null Thing = null
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
**Hint:** You can use bit-wise operations to manipulate `ReportMode`:  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```
##### ShrinkMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from relevant in Checkr.Input("Relevant", Fuzzr.Int(), Reduce.TowardsZero)
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
    from expect in Checkr.Expect("Third Time Answer", () => !(counter == 3 && relevant >= 42))
    select Case.Closed;
```
###### ShrinkMode.None
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
QuickCheckr reports the original failing run.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeNone.cs:68:1
 Original failing run:    3 executions
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 1
   - Input: Relevant   = 51
   - Input: Irrelevant = 6
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Relevant   = 38
   - Input: Irrelevant = 19
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Relevant   = 66
   - Input: Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
###### ShrinkMode.Executions
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
Trims away irrelevant executions.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeExecutions.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Relevant   = 66
   - Input: Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
###### ShrinkMode.Inputs
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
Trims inputs to find a minimal failing case.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeInputs.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 7 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed: (2 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed:
   - Input: counter  = 3
   - Input: Relevant = 66
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
###### ShrinkMode.Reduction
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs | ShrinkMode.Reduction };
```
Applies configured reducers for fine-grained shrinking.  

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
*Note:* For this mode to have any effect `ShrinkMode.Inputs` also needs to be activated.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeReduction.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 8 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed: (2 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed:
   - Input: counter  = 3
   - Input: Relevant = 66
            Redux      42
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
###### ShrinkMode.Default
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Default };
```
Includes all possible ShrinkModes.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeDefault.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter  = 3
   - Input: Relevant = 66
            Redux      42
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
**Hint:** You can use bit-wise operations to manipulate `ShrinkMode`:  
```csharp
config with { ShrinkMode = config.ShrinkMode & ~ShrinkMode.Reduction };
```
### The Evidence Vault
[The Board][PuttingACaseOnTheBoard] | [Previous][Configuration] | [Next][GatheringEvidence]  
  
#### Reviewing Cold Cases
Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.  
```csharp
checkr.ReviewColdCases(Configure);
```
This will create a report per seed in the evidence vault and a log of all evidence vault actions.  
- For every failing seed the report will look like the by now familiar failure report.  
- For every passing seed the report will only contain a list of passed expectations.  
#### Closing Cold Cases
The method mentioned above is usefull for regression testing, but sometimes,
    there is no need to keep the evidence around after the culprit is found.  
    In this case we use:  
```csharp
checkr.CloseResolvedColdCases(Configure);
```
If the seeds stored in the vault still all result in failure, the behaviour is exactly the same as `ReviewColdCases`.  
If however a run for a given seed no longer fails, the seed is removed from the evidence vault,
    and no report is generated for the passing run.  
    Any existing previous reports for runs that now pass are deleted, and if the evidence vault is empty, it is also deleted.  

[The Board][PuttingACaseOnTheBoard] | [Previous][Configuration] | [Next][GatheringEvidence]  

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[Configuration]: #configuration

[GatheringEvidence]: #gathering-evidence
### Gathering Evidence
[The Board][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  
  
This is another way to run a Checkr, and an alternate method to populate the *Evidence Vault* with seeds for failing runs.  
#### Running Investigations
```csharp
checkr.GatherEvidence(50.Investigations(), 3.Runs(), 10.ExecutionsPerRun(),
    cfg => cfg with { FileAs = "GatheringEvidence" });
```
When you use the above method the following steps are taken:
- The existing evidence from the vault is loaded (if available), the seeds are run, and the failing casefiles are kept in memory.
- Next we run the specified number of Investigations, and any new failing case file is added to the in-memory list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- And finally, all evidence and reports are saved to the vault.

**Note:** An *Investigation* is a new kind of scope.  
Running a Checkr in the *normal* way will always produce one report, even if used with a DeliberationPolicy.  
When you're gathering evidence, a report is generated per Investigation. This allows for finding different bugs in the same system,
or for documenting the different ways a single bug can manifest.

As stated above, similar CaseFiles are not reported in order to increase the signal to noise ratio.  
  
#### Gathering Evidence Case File Compare
**Casefiles are considered *not similar* when:**  
- *Failure Description* is not equal.  
- *Execution Count* is not equal.  
- Any **Execution** differs, like so:  
  - Number of *Actions Performed* is not equal.  
  - Any *Action label* is not equal.  
  - Number of *Inputs* is not equal.  
  - Any *Input label* is not equal.  

Per default, the order of Executions does not matter for similarity.  
An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.  
#### Show, Don't Tell
As stated in [the guide][Guide] we're going to use the [Bolting Horses][BoltingHorses] example,
to demonstrate how to find all the different ways the invariant can be violated.

So we're using the same model, and *almost* exactly the same Checkr.  
Changes:
- Added Shrink.None to the Name inputs. 
- Changed the `UpdateCoachSkills` a little to avoid the issue documented [here][J_TheCaseOfTheInvisibleCollection]:  
```csharp
public class GatheringEvidenceCheckr
{
    public static CheckrOf<Case> TheCheckr() =>
        from coaches in Trackr.PoolFor<Coach>()
        from courses in Trackr.PoolFor<Course>()
        from operations in Checkr.OneOfWhen(
            Trackr.PoolWhen<Coach>(a => a.Count < 10, CreateCoach),
            Trackr.PoolWhen("Coach to Update Skills", a => a.Count != 0, UpdateCoachSkills),
            Trackr.PoolWhen<Course>(a => a.Count < 5, CreateCourse),
            Trackr.PoolWhen("Course to Update Skills", a => a.Count != 0, UpdateCourseSkills),
            Trackr.PoolWhen("(Coach, Course) to Assign",
                (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;

    private static readonly CheckrOf<Case> CreateCoach =
        from name in Checkr.Input("Coach Name", NameFuzzr.For("Coach"), Shrink.None<string>)
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.ToPool("Store Coach", coach)
        select Case.Closed;

    private static readonly Func<PoolElement<Coach>, CheckrOf<Case>> UpdateCoachSkills =
        coach =>
            from skills in Checkr.Input("Coach Skills"
                , Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3)
                , () => [
                    Shrink.Default<IEnumerable<string>>(),
                    Shrink.Collection([[.. coach.Value.Skills]]) ]
                , Reduce.Collection([[], [.. coach.Value.Skills]]))
            from updateSkills in Checkr.Act("Update Coach Skills",
                () => coach.Value.UpdateSkills(skills))
            from checkSkills in Checkr.Expect("Coach Skills Set",
                () => coach.Value.Skills.Count
                   == skills.Count() && coach.Value.Skills.All(skills.Contains))
            select Case.Closed;

    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name", NameFuzzr.For("Course"), Shrink.None<string>)
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course)
        select Case.Closed;

    private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
        course =>
            from skills in Checkr.Input("Course Skills",
                Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            from updateSkills in Checkr.Act("Update Course Skills",
                () => course.Value.UpdateRequiredSkills(skills))
            from checkSkills in Checkr.Expect("Course Required Skills Set",
                () => course.Value.RequiredSkills.Count
                   == skills.Count() && course.Value.RequiredSkills.All(skills.Contains))
            select Case.Closed;

    private static readonly Func<PoolElement<Coach>, PoolElement<Course>, CheckrOf<Case>> AssignCoachToCourse =
        (coach, course) =>
            from assign in Checkr.Act("Assign Coach to Course",
                () => course.Value.AssignCoach(coach.Value))
            from isAssigned in Checkr.Expect("Coach is assigned",
                () => course.Value.AssignedCoach == coach.Value)
            from hasCourse in Checkr.Expect("Coach has Course",
                () => coach.Value.AssignedCourses.Contains(course.Value))
            select Case.Closed;

    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from assignedCheck in Trackr.PoolExpectEachWhen<Coach>("Assigned Coach Must Be Suitable",
            coach => coach.AssignedCourses.Any(),
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
        select Case.Closed;
}
```
For the code on display here, running this checkr using the code snippet at the top of this doc, 
consistently results in two different reports.  
```text
------------------------------------------------------------
 Original failing run:    10 executions
 Minimal failing case:    4 executions (after 11 shrinks)
 Seed:                    1652806413
 ------------------------------------------------------------
  Executed: Create Course
   - Store Course = Course-1
 ------------------------------------------------------------
  Executed: Create Coach
   - Store Coach = Coach-3
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - (Coach, Course) to Assign = ( Coach-3, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Course to Update Skills = Course-1
   - Course Skills           = [ _ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Coach Name is set: 3x
 - Coach Skills Set: 2x
 - Course Name Set: 1x
 - Course Required Skills Set: 3x
 - Coach is assigned: 1x
 - Coach has Course: 1x
 - Assigned Coach Must Be Suitable: 1x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    12 executions
 Minimal failing case:    6 executions (after 11 shrinks)
 Seed:                    414682113
 ------------------------------------------------------------
  Executed: Create Course
   - Store Course = Course-1
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Course to Update Skills = Course-1
   - Course Skills           = [ "EF Core" ]
 ------------------------------------------------------------
  Executed: Create Coach
   - Store Coach = Coach-2
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Coach to Update Skills = Coach-2
   - Coach Skills           = [ "EF Core" ]
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - (Coach, Course) to Assign = ( Coach-2, Course-1 )
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Coach to Update Skills = Coach-2
   - Coach Skills           = [ "TDD" ]
     Redux                    [ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Course Name Set: 2x
 - Coach Name is set: 2x
 - Course Required Skills Set: 2x
 - Coach is assigned: 4x
 - Coach has Course: 4x
 - Assigned Coach Must Be Suitable: 6x
 - Coach Skills Set: 2x
 ------------------------------------------------------------
```
#### An Observation
When I first implemented this feature, I was quite puzzled.
Surely there are more ways this Checkr can fail, I thought.  

It took a bit of investigating as to why this is not the case. Let me give you an example of what is going on.

If you look at the second report, the one with 6 executions, surely there is a similar case,
but instead of updating the Coach skills, we update the Course skills and that would also fail.
Now that's a new way of failing the Checkr, right?

Wrong!  

Below is the report of the other failure (the one with 4 executions) with shrinking disabled,
but I'll save you the *bit of investigating* and give you a recap:
- Execution 03: Create Course-1
- Execution 06: Create Coach-3
- Execution 07: Update Coach Skills to [ "DomainDrivenDesign", "JavaScript" ]
- Execution 08: Update Course Skills to [ "DomainDrivenDesign" ]
- Execution 09: Assign Coach-3 to Course-1
- Execution 10: Update Course Skills to [ "JavaScript", "TDD", "CI/CD" ]

As you can see, this is exactly the case described above.  
The Shrinker however managed to collapse that one down to only four executions, so it is in fact covered by the first report.
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\B_PuttingACaseOnTheBoard\E_GatheringEvidence\GatheringEvidenceObservation.cs:37:1
 Original failing run:    10 executions
 Seed:                    1652806413
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-1
   - Input: Coach Name  = "Coach-1"
 ------------------------------------------------------------
  Executed: Update Coach Skills
   -  Pool: Coach to Update Skills = Coach-1
   - Input: Coach Skills           = [ "ASP.NET" ]
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: Store Course = Course-1
   - Input: Course Name  = "Course-1"
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-2
   - Input: Coach Name  = "Coach-2"
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "UnitTesting" ]
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-3
   - Input: Coach Name  = "Coach-3"
 ------------------------------------------------------------
  Executed: Update Coach Skills
   -  Pool: Coach to Update Skills = Coach-3
   - Input: Coach Skills           = [ "DomainDrivenDesign", "JavaScript" ]
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "DomainDrivenDesign" ]
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   -  Pool: (Coach, Course) to Assign = ( Coach-3, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "JavaScript", "TDD", "CI/CD" ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Coach Name is set: 3x
 - Coach Skills Set: 2x
 - Course Name Set: 1x
 - Course Required Skills Set: 3x
 - Coach is assigned: 1x
 - Coach has Course: 1x
 - Assigned Coach Must Be Suitable: 1x
 ------------------------------------------------------------
```

[The Board][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[TheEvidenceVault]: #the-evidence-vault

[Guide]: #guide

[BoltingHorses]: #bolting-horses

[J_TheCaseOfTheInvisibleCollection]: #the-case-of-the-invisible-collection
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
 Test:                    Example
 Location:                ExecutionShrinking.cs:68:1
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
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:39:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    165641598
 ------------------------------------------------------------
  Executed:
   - WARNING: All inputs were considered irrelevant.
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
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:81:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    231122398
 ------------------------------------------------------------
  Executed: Result
   - Input: Input One   = 12
   - Input: Input Three = 30
 ===========================================
  !! Expectation Failed: Should not be 42
 ===========================================
```
Many more examples focused on shrinking behaviour can be found [here][HoneyIShrunkTheInputs].  

[HoneyIShrunkTheInputs]: #honey-i-shrunk-the-inputs
### Input Reduction

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
After InputShrinking has classified all inputs as either relevant or irrelevant, any registered *reducers*
are run on the remaining relevant inputs (example below).  

Note the use of the word *registered*. Staying true to it's shrinking philosophy,
QuickChecker does not try and guess how to reduce input values. It's up to the user to supply that information.  

Most of the time, even in the example here I think, reducers are not needed to figure out what went wrong from the report.  
But in some cases it is a must.  

So what is a reducer ?  

Well let me show you.  
Using the `Account` example from the guide:  

**The Model:**  
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```

**The Checkr:**  
We're running the same checkr, with the same seed.
The only difference is the addition of `Reduce.TowardsZero` in the `Checkr.Input` argument list.  
```csharp
from account in Trackr.Stashed(() => new Account())
from amount in Checkr.Input("Amount", Fuzzr.Int(), Reduce.TowardsZero) // <=
from operations in Checkr.OneOf(
    Checkr.Act("Deposit", () => account.Deposit(amount)),
    Checkr.Act("Withdraw", () => account.WithDraw(amount)))
from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                InputReduction.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1684147193
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 87
            Redux    1
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 1x
 ------------------------------------------------------------
```
And as you can see it does exactly what it says on the tin. 
It *reduces* the original value using a user supplied algorithm.  
This one just decrements the input until (and including) it reaches zero, and checks if the run still fails.
If the run passes, the value last tried is reported as the *reduced* input.  

In the example here, `0` was tried, but it made the test pass, so the previously tried candidate `1` is reported.  

QuickCheckr comes with a small number of built-in reducers, but it is relatively easy to implement your own.  
Depending on demand, the library of reducers might grow fast.
  
### Deliberation Shrinking
This next step, which is disabled by default, allows the shrinker to find *global* minimal failcases instead of *local* ones.  

A first example, as well as how to enable this feature,
can be found in the entry on Deliberation Policies in the [Configuration][Configuration] chapter.

But here are some more use cases.  
#### An Eager Shrinker
Under some rare circumstances, the Shrinker gets *greedy*.  
One currently identified case where this happens is if more than one input triggers the same failure.  
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
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:41:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    227109555
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, _ ]
   - Trace: List  = [ 1, 2, 1 ]
 =============================================
  !! Expectation Failed: Does not contain 1
 =============================================
```
It seems QuickCheckr thinks that the minimal fail-case is a list with atleast 3 elements (*correct*),
but the element values are considered irrelevant (*wrong*).  
For this rare case we  add a DeliberationPolicy, allowing a more explanatory fail-case to surface:  
```csharp
private static CheckrConfig ConfigureShrinkCount(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumShrinkCount };
```
**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyFixed
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:82:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1133636239
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, 1 ]
   - Trace: List  = [ 0, 4, 1 ]
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
**With Deliberation Policy:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyObjects
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:105:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    149961996
 ------------------------------------------------------------
  Executed:
   - Input: BaggyInters = { IntThree: 1 }
   - Trace: Original    = { IntOne: 3, IntTwo: 2, IntThree: 1 }
 ===============================================================
  !! Expectation Failed: obj.IntOne == 1 || obj.IntThree == 1
 ===============================================================
```

[Configuration]: #configuration
## You Can Go Your Own Way

!! WARNING: **Unfinished Documentation** !!

> This feature is finished, the tests are there.  
>  
> But ..., we're working on writing down the explanation.  
> Depending on how far we are in this process, you could just see some scaffolding files here.  
> Or the raw output from the Unit Tests.  
> Or an unfinished, unpolished version of the docs.
>
> Come back soon.
  
We already saw in the entry on [Configuration][Configuration] how you can turn some knobs to customize QuickCheckr's behaviour
and/or reporting globally.  
But there are ways to obtain more fine-grained control. That is what this chapter is about.  
### Contents

- [Showr][TheShowrClass]
- [Custom Shrinkers][CustomShrinkers]
- [Reducers][Reducers]
  

[Configuration]: #configuration

[A_TheShowrClass]: #the-showr-class

[CustomShrinkers]: #custom-shrinkers

[Reducers]: #reducers
### The Showr Class
This static factory class produces a CheckrOf<Case> which allows one to configure the pretty-printer used in the report.

It is implemented as such, instead of as an option on the configuration object,
because this allows for two useful things:
- You can change configuration anywhere in the `LINQ chain`, multiple times.
- Configuration can depend on context.
  
```csharp
    from acc in Trackr.Stashed(() => new Accumelator())
    from count in Checkr.Input("Counter", Fuzzr.Counter("count"))
    from show in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"Int {a}"))
    from a in Checkr.Input("a", Fuzzr.Int())
    from b in Checkr.Input("b", Fuzzr.Int())
    from c in Checkr.Input("c", Fuzzr.Int())
    from add in Checkr.Act("Add", () => acc.Add(a, b))
    from notAnswerOnTwo in Checkr.Expect("not 42 on execution 2", () => !(count == 2 && acc.Total == 42))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\J_YouCanGoYourOwnWay\A_Showr\TheShowrClass.cs:20:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 3 shrinks)
 Seed:                    344916298
 ------------------------------------------------------------
  Executed: Add
   - Input: a = 8
                Int 8
   - Input: b = 16
                Int 16
 ------------------------------------------------------------
  Executed: Add
   - Input: Counter = 2
   - Input: a       = 1
                      Int 1
   - Input: b       = 17
                      Int 17
 ================================================
  !! Expectation Failed: not 42 on execution 2
 ================================================
 Passed Expectations
 - not 42 on execution 2: 1x
 ------------------------------------------------------------
```
#### Showr Reference
##### For Trace

**The Checkr:**  
```csharp
from showr in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = PREFIXED 7
```
##### For Input

**The Checkr:**  
```csharp
from showr in Showr.ForInput(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = 7
```
##### For Input Without Configuration

**The Checkr:**  
```csharp
from showr in Showr.ForInput()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    7
   - Trace: AnInt = 7
```
##### For All

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = PREFIXED 7
```
##### None

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from showrNone in Showr.None()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = 7
```
#### None

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from showrNone in Showr.None()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = 7
```
### Custom Shrinkers

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
#### Registered But Unused
Because custom shrinkers can be defined for *nested* types, mistakes are possible.
Hence this warning.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), () => Shrink.Values(["unused"]))
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
#### Shrink Default

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Shrink.Default<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkDefault.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
#### Shrink Function

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    // The lamda argument is the original Input value
    () => Shrink.Function((Thing a) => [new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkFunction.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
#### Shrink Fuzzr

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!), () => Shrink.Fuzzr(Fuzzr.One<Thing>(), 2))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkFuzzr.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
#### Shrink Multiple

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => [Shrink.Values([new Thing()]), Shrink.Function((Thing a) => [new Thing()])])
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkMultiple.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
#### Shrink None

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Shrink.None<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```
Weird option this one at first glance, maybe. It does not make inputs turn up in the report.  
We know it works because of the shrink count.
No shrinking => no shrinking clue => not in report.
This is correct behavior.
  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkNone.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
#### Shrink Values

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Shrink.Values([new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkValues.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
### Reducers

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
Reducers are a relatively new addition to the QuickCheckr arsenal.  
And you could, rightfully so, point out that they violate it's core philosophy
of not rewriting what actually happened.  

But once again, it is a feature born out of necessity.  
 
With that caveat out of the way, let's dig in.  
  
#### Values Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Values([3, 2, 1, 0]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_ValuesStopOnFirstPassed.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
#### Values Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Values([3, 0, 1, 2]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_ValuesStopOnFirstPassedAlternative.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   3
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
#### Values Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Values([3, 0, 1, 2], true))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_ValuesReportLastFailure.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
#### Collection
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5), Reduce.Collection([[3], [2], [1], [0]]))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_Collection.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ 94 ]
            Redux   [ 2 ]
 ==================================
  !! Expectation Failed: All < 2
 ==================================
```
#### Collection Of
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5), Reduce.CollectionOf<int>(a => [[3], [2], [1], [0]]))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_CollectionOf.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ 94 ]
            Redux   [ 2 ]
 ==================================
  !! Expectation Failed: All < 2
 ==================================
```
#### Function Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Function<int>(a => [3, 2, 1, 0]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_FunctionStopOnFirstPassed.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
#### Function Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Function<int>(a => [3, 0, 1, 2]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_FunctionStopOnFirstPassedAlternative.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   3
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
#### Function Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Function<int>(a => [3, 0, 1, 2], true))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_FunctionReportLastFailure.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
#### Reduce Towards Zero

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.TowardsZero)
from expect in Checkr.Expect("Input should be smaller than 42", () => input < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_ReduceTowardsZero.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   42
 ==========================================================
  !! Expectation Failed: Input should be smaller than 42
 ==========================================================
```
#### Reduce Minimize String

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(), Reduce.MinimizeString)
from expect in Checkr.Expect("no a and b", () => !(text.Contains('a') && text.Contains('b')))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_ReduceMinimizeString.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1834440365
 ------------------------------------------------------------
  Executed:
   - Input: Text  = "tdblaulb"
            Redux   "ab"
 =====================================
  !! Expectation Failed: no a and b
 =====================================
```
#### Custom

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), new CustomTowardsZeroReducer().Reduce)
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Custom Reducer:**  
```csharp
public class CustomTowardsZeroReducer
{
    private int lastFailed = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var value = result.ValueAs<int>();
        if (result.Failed)
            lastFailed = value;
        else
            return Reduction.Stop(lastFailed);
        if (value == 0)
            return Reduction.Stop(lastFailed);
        if (value > 0)
            value--;
        else
            value++;
        return Reduction.Next(value);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Custom.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    211010750
 ------------------------------------------------------------
  Executed:
   - Input: Input = 10
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
## Honey I Shrunk the Inputs
This is a collection of simple shrinking examples organized per type.  
They serve both as an accurate description of input shrinking behaviour, and as regression tests.  

So if you file an issue regarding input shrinking, you will likely see a new example added here.  
### Primitives
By default QuickCheckr does not *mutate* primitive input values, it only marks them as relevant or irrelevant  
#### Built In Enumerations

**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<DayOfWeek>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<DayOfWeek>())
from traceOne in Checkr.Trace("Original One", () => inputOne)
from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
from expect in Checkr.Expect("Two should not be Monday", () => inputTwo != DayOfWeek.Monday)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_BuiltInEnumerations.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2083214969
 ------------------------------------------------------------
  Executed:
   - Input: Two          = Monday
   - Trace: Original One = Wednesday
   - Trace: Original Two = Monday
 ===================================================
  !! Expectation Failed: Two should not be Monday
 ===================================================
```
#### Custom Enumerations

**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<SoundCheck>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<SoundCheck>())
from traceOne in Checkr.Trace("Original One", () => inputOne)
from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
from expect in Checkr.Expect("Two should not be Two", () => inputTwo != SoundCheck.Two)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CustomEnumerations.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1735186993
 ------------------------------------------------------------
  Executed:
   - Input: Two          = Two
   - Trace: Original One = Two
   - Trace: Original Two = Two
 ================================================
  !! Expectation Failed: Two should not be Two
 ================================================
```
### Collections
When shrinking collections, QuickCheckr does not attempt to rewrite or regenerate the list.
Instead, it removes items one by one,
replaying the run each time to test whether the removed element was relevant for the failure.

This means that if only some elements in a collection contribute to the failing condition,
QuickCheckr will aggressively trim the rest away.
The result is a minimal collection containing only the values that still trigger the failure,
even if the original list was large or noisy.
  
#### Removing Irrelevant Items
Consider a test that forbids the value `7`.  

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
from expect in Checkr.Expect("List does not contain 7", () => !listInput.Contains(7))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_RemovingIrrelevantItems.cs:59:1
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
Even if the original list was long and varied, QuickCheckr isolates the single relevant element:  
#### Removing Irrelevant Collection
Ofcourse a collection itself is just an input like another, so if it, as a whole,
is irrelevant for the failure, QuickCheckr will remove it.  

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
from expect in Checkr.Expect("Always fails", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_RemovingIrrelevantCollection.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2014608033
 ------------------------------------------------------------
  Executed:
   - WARNING: Empty collection deemed irrelevant at input 'List Input'.
   - WARNING: All inputs were considered irrelevant.
 =======================================
  !! Expectation Failed: Always fails
 =======================================
```
#### Irrelevant Element But Collection Length Is Not
When shrinking collections, QuickCheckr isolates the structural aspects that contributed to the failure
and leaves irrelevant details untouched. 

In cases like this example where the predicate depends solely on the length of the collection,
the individual element values play no role.  
QuickCheckr therefore preserves them as anonymous placeholders (_) and concentrates entirely on reducing the collection's shape:
removing elements, and converging toward the smallest list that still violates the condition.

This yields minimal, focused shrinking artifacts:
the report shows only the structural signal that mattered (here: the list being *too long*),
without introducing noise from element-level changes that could not affect the outcome anyway.  

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
from trace in Checkr.Trace("Original List", () => list)
from expect in Checkr.Expect("Length <= 2", () => list.Count() <= 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_IrrelevantElementButCollectionLengthIsNot.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1393576429
 ------------------------------------------------------------
  Executed:
   - Input: List          = [ _, _, _ ]
   - Trace: Original List = [ 12, 45, 56, 18, 31 ]
 ======================================
  !! Expectation Failed: Length <= 2
 ======================================
```
#### Multiple Relevant Elements

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
from list in Checkr.Capture(() => input.ToList())
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("One, Two and Three", () => input.Count() > 2, () =>
     !(list[0] == 1
    && list[1] == 2
    && list[2] == 3))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                F_MultipleRelevantElements.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    500957855
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 1, 2, 3 ]
   - Trace: Original = [ 1, 2, 3, 9, 3, 1 ]
 =============================================
  !! Expectation Failed: One, Two and Three
 =============================================
```
#### Relevant Element And Size

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen("Second element is not 7",
    () => input.Count() > 1, () => input.ElementAt(1) != 7)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                G_RelevantElementAndSize.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1374380958
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ _, 7 ]
   - Trace: Original = [ 6, 7, 9, 7, 3, 2 ]
 ==================================================
  !! Expectation Failed: Second element is not 7
 ==================================================
```
#### Nested Collections

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen("List[0][0] is not 42",
    () => input.Any() && input.First().Any(),
    () => input.First().First() != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H_NestedCollections.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    146712596
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ [ 42 ] ]
   - Trace: Original = [ [ 42, 1, 15 ], [ 17 ], [ 27, 9 ] ]
 ===============================================
  !! Expectation Failed: List[0][0] is not 42
 ===============================================
```
#### When An Empty Collection Is The Culprit

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Int().Many(0, 10))
from trace in Checkr.Trace("Original", () => list)
from listNotEmpty in Checkr.Expect("List is not empty", () => list.Any())
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                I_WhenAnEmptyCollectionIsTheCulprit.cs:59:1
 Original failing run:    1 execution
 Seed:                    2077876521
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ ]
   - Trace: Original = [ ]
 ============================================
  !! Expectation Failed: List is not empty
 ============================================
```
#### The Case Of The Invisible Collection
##### The Issue
Another case of '*Greedy Shrinker Disease*'.  

This one is caused by the fact that the shrinker only sees the inputs.
It is unaware of any *state derived* meaningfull values.   

**The Model:**  
```csharp
public class Thing
{
    public List<int> Ints = [];
}
```

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from _ in Checkr.OneOfWhen(
    Checkr.Option(() => !thing.Ints.Contains(1),
        from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
        from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints", Fuzzr.Int().Many(2))
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
        select Case.Closed))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J1_TheCaseOfTheInvisibleCollection.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
   - WARNING: Empty collection deemed irrelevant at input 'Ints'.
   - WARNING: All inputs were considered irrelevant.
 ================================
  !! Expectation Failed: Has 1
 ================================
```
##### The Solution
Supply said *state derived* meaningfull values, through a combination of a custom Shrinker and a Reducer.  

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from _ in Checkr.OneOfWhen(
    Checkr.Option(() => !thing.Ints.Contains(1),
        from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
        from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints",
    Fuzzr.Int().Many(2),
    () => [
        Shrink.Default<IEnumerable<int>>(),
        Shrink.Collection([thing.Ints])],
    Reduce.Collection([[], thing.Ints]))
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
        select Case.Closed))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J2_TheCaseOfTheInvisibleCollection.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
   - Input: Ints  = [ 57, 70 ]
            Redux   [ ]
 ================================
  !! Expectation Failed: Has 1
 ================================
```
#### A Collection Of Objects

**The Model:**  
```csharp
public class Thing
{
    public int One { get; set; }
    public int Two { get; set; }
}
```

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.One<Thing>().Many(2, 5))
from show in Showr.ForTrace(ListShowr)
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("Only answers for One",
    () => list.Any(), () => list.All(a => a.One != 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                K_ACollectionOfObjects.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    849984174
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ { One: 42 } ]
   - Trace: Original = [
    { One: 13, Two: 49 },
    { One: 61, Two: 79 },
    { One: 93, Two: 91 },
    { One: 18, Two: 39 },
    { One: 42, Two: 54 }
]
 ===============================================
  !! Expectation Failed: Only answers for One
 ===============================================
```
### Objects
#### Checking Nulls
**The Model:**  
```csharp
public class TheUniverse
{
    public class Life { public int Answer { get; set; } }
    public Life? Everything { get; set; }
    public Life? Irrelevant { get; set; }
    public int GetTheAnswer() => Everything!.Answer;
}
```
**The Checkr:**  
```csharp
    from universe in Checkr.Input("The Universe", TheUniverseFuzzr)
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:36:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    358877731
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 78 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
Note the shrinking of the `Irrelevant` property.  
In addition, QuickCheckr reports a shrinking warning:  
> Null value for type 'Life' at input 'The Universe.Everything'.  
This tells you that the shrinker reached a `null` value that it cannot safely reduce any further.  
  
But when both `Everything` *and* `Irrelevant` are `null`, QuickCheckr cannot shrink either property, and both limitations are reported as warnings:  
```text
------------------------------------------------------------
 Test:                    RunTest_NotShrinkingNull
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:85:1
 Original failing run:    1 execution
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null, Irrelevant: null }
   - Trace: The Universe = { Everything: null, Irrelevant: null }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
   - WARNING: Null value for type 'Life' at input 'The Universe.Irrelevant'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
We could help the shrinker by rewriting your checkr, in order to avoid the irrelevant input being reported as relevant.  
```csharp
    from everything in Checkr.Input("Everything", TheLifeFuzzr, () => Shrink.Fuzzr(TheLifeFuzzr))
    from irrelevant in Checkr.Input("Irrelevant", TheLifeFuzzr, () => Shrink.Fuzzr(TheLifeFuzzr))
    from universe in Checkr.Capture(() => new TheUniverse { Everything = everything, Irrelevant = irrelevant })
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Alternative
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:117:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: Everything   = null
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
Or, we can solve the issue using a [`DeliberationPolicy`][DeliberationShrinking] that limits the number of warnings:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumWarnings };
```
```text
------------------------------------------------------------
 Test:                    RunTest_WithDeliberationPolicy
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:165:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1643721177
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 19 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
As you can see the warning is still there for `Universe.Everything` but this is a relevant input,
so the warning can be ignored.  
Deliberation did succeed in removing the `Universe.Irrelevant` input from the report.  

[DeliberationShrinking]: #deliberation-shrinking
