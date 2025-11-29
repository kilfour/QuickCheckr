# Bolting Horses
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
 Location:                Docs\A_Guide\E_HorsesAgain\BoltingHorses.cs:59:1
 Original failing run:    20 executions
 Minimal failing case:    4 executions (after 21 shrinks)
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
The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment.  
