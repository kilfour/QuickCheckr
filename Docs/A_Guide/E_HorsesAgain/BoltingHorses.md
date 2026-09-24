# Bolting Horses
|[◀][InPoolsWeTrust]|[△][Guide]|▶|
|---|---|---|
  
Up to this point we have seen a few examples of how to use QuickCheckr to test a system, but the bugs have been relatively trivial.
For this one, we're going to look at a real bug found in the wild, which is demonstrative for a whole category: *Invariants drifting over time*.

> This is the kind of bug QuickCheckr is built to find.

*So, here we go:*  

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
    from toPool in Trackr.ToPool("Store Coach", () => coach) // Store this coach in the pool.
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
    from toPool in Trackr.ToPool("Store Course", () => course) // Store this course in the pool.
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
**Assign Coach to Course Checkr:**  
```csharp
private static readonly Func<PoolElement<Coach>, PoolElement<Course>, CheckrOf<Case>> AssignCoachToCourse =
    (coach, course) =>
        from assign in Checkr.Act("Assign Coach to Course", () => course.Value.AssignCoach(coach.Value))
        from isAssigned in Checkr.Expect("Coach is assigned", () => course.Value.AssignedCoach == coach.Value)
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
 Location:                BoltingHorses.cs:82:1
 Original failing run:    13 executions
 Minimal failing case:    4 executions (after 14 shrinks)
 Seed:                    1473455315
 ------------------------------------------------------------
  Executed: Create Course
   - Store Course = Course-1
 ------------------------------------------------------------
  Executed: Create Coach
   - Store Coach = Coach-1
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - (Coach, Course) to Assign = ( Coach-1, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Course to Update Skills = Course-1
   - Course Skills           = [ _ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
       Coach-1
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

  

[GatheringEvidence]: ../../../Drafts/B_PuttingACaseOnTheBoard/E_ConductingInvestigations/GatheringEvidence.md

[Guide]: ../Guide.md

[InPoolsWeTrust]: ../D_InPoolsWeTrust/InPoolsWeTrust.md
