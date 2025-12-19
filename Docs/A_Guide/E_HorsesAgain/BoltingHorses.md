# Bolting Horses
Up to this point we have seen a few examples of how to use QuickCheckr to test a system, but the bugs have been relatively trivial.
For this one, we're going to look at a real bug found in the wild, which is demonstrative for a whole category: *Invariants drifting over time*.

The only concept we haven't covered yet, and that we need here, is:

**Pools**  
These let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  
In this example, we'll keep all created coaches and courses in pools,
and each operation will pick from those pools to mutate the system over time.

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, pool operations (When, These, ToPool, ...) encapsulate "is this state valid?" so shrinking can replay safely.

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
        coaches.When(a => a.Count() < 10, CreateCoach),
        // Maybe update existing coaches. Pick a random coach from the pool.
        coaches.When(a => a.Any(), "Coach to Update Skills", UpdateCoachSkills),
        // Create up to 5 courses.
        courses.When(a => a.Count() < 5, CreateCourse),
        // Maybe update existing courses. Pick a random course from the pool.
        courses.When(a => a.Any(), "Course to Update Skills", UpdateCourseSkills),
        // Maybe assign coach if possible.
        // Pick a random coach and course, if there exists a combination for which the predicate holds.
        Pool.These(coaches, courses).When("Assign Coach to Course",
            (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
        // Ensure the system invariant is met at all times.
    from invariant in AssignedCoachesMustBeSuitable(coaches)
    select Case.Closed;
```
**Create Coach Checkr:**  
In this one we create a coach with a *semi* random name, verify that the property `Name` is assigned and then add it to the coaches pool.  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from trace in Checkr.Trace("Coach Name", () => name)
    from coach in Checkr.Act("Create Coach", () => new Coach(name))
    from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
    from toPool in Trackr.ToPool("Store Coach", coach) // Store this coach in the pool.
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
Now we retrieve a random coach from the pool and update its skills, verifying that the property `Skills` is set correctly.  
```csharp
private static readonly Func<Coach, CheckrOf<Case>> UpdateCoachSkills =
    coach =>
        from skills in Checkr.Input("Coach Skills",
            Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from trace in Checkr.Trace("Coach Name", () => coach.Name)
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => coach.UpdateSkills(skills)) // Update the Coach.
        from checkSkills in Checkr.Expect("Coach Skills Set",
            () => coach.Skills.Count
               == skills.Count() && coach.Skills.All(skills.Contains))
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
    from toPool in Trackr.ToPool("Store Course", course) // Store this course in the pool.
    select Case.Closed;
```
**Update Course Skills Checkr:**  
And again, same pattern as the *Update Coach Skills Checkr*.  
```csharp
private static readonly Func<Course, CheckrOf<Case>> UpdateCourseSkills =
    course =>
        from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from traceCourse in Checkr.Trace("Course Name", () => course.Name)
        from updateSkills in Checkr.Act("Update Course Skills",
            () => course.UpdateRequiredSkills(skills)) // Update the Course.
        from checkSkills in Checkr.Expect("Course Required Skills Set",
            () => course.RequiredSkills.Count
               == skills.Count() && course.RequiredSkills.All(skills.Contains))
        select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
And finally here's the system invariant.  
*Any coach assigned a course must have the required skills to teach that course*.  
```csharp
private static CheckrOf<Case> AssignedCoachesMustBeSuitable(Pool<Coach> coaches) =>
    from assignedCheck in coaches.ExpectForEachWhen("Assigned Coach Must Be Suitable",
        coach => coach.AssignedCourses.Any(), // Check that all assigned coaches are suitable.
        coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\A_Guide\E_HorsesAgain\BoltingHorses.cs:83:1
 Original failing run:    15 executions
 Minimal failing case:    4 executions (after 18 shrinks)
 Seed:                    1073869613
 ------------------------------------------------------------
  Executed: Create Coach
   - Trace: Coach Name = Coach-3
 ------------------------------------------------------------
  Executed: Create Course
   - Trace: Course Name = Course-1
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - Trace: Coach Name = Coach-3
   - Trace: Course Name = Course-1
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Input: Course Skills = [ _ ]
   - Trace: Course Name = Course-1
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Assigned Coach Must Be Suitable: 14x
 - Coach Name is set: 5x
 - Course Name Set: 1x
 - Coach is assigned: 3x
 - Coach Skills Set: 1x
 - Course Required Skills Set: 1x
 ------------------------------------------------------------
```
The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment.  
