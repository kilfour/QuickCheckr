# Lock The Stable Doors
**The Checkr:**  
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from service in Trackr.Stashed(() => new InMemoryCoachCourseService())
    from coaches in Trackr.PoolFor<CoachBy>()
    from courses in Trackr.PoolFor<CourseBy>()
    from dbCourses in Trackr.Capture(service.GetCoursesAsync())
    from operations in Checkr.OneOfIf(
        (() => coaches.Count() < 10, CreateCoach),
        (() => coaches.Any(), UpdateCoachSkills),
        (() => courses.Count() < 5, CreateCourse),
        (() => courses.Any(), UpdateCourseSkills),
        (() => coaches.Any() && dbCourses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
    from invariant in AssignedCoachesMustBeSuitable
    select Case.Closed;
```
**Create Coach Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from trace in Checkr.Trace("Coach Name", () => name)
    from coach in Checkr.Act("Create Coach", service.CreateCoachAsync(name))
    from toPool in Trackr.Pool("Store Coach", new CoachBy(name))
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCoachSkills =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from coach in Trackr.OneOfPool<CoachBy>("Coach to update")
    from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from valid in Checkr.When(() => coach is not null,
        from trace in Checkr.Trace("Coach Name", () => coach.Name)
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => service.UpdateCoachSkillsAsync(coach.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Create Course Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCourse =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Course Name",
        from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
    from trace in Checkr.Trace("Course Name", () => name)
    from course in Checkr.Act("Create Course", service.CreateCourseAsync(name))
    from toPool in Trackr.Pool("Store Course", new CourseBy(name))
    select Case.Closed;
```
**Update Course Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCourseSkills =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from course in Trackr.OneOfPool<CourseBy>("Course to update")
    from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from courseNotNull in Checkr.When(() => course is not null,
        from traceCourse in Checkr.Trace("Course Name", () => course.Name)
        from updateSkills in Checkr.Act("Update Course Skills",
            service.UpdateCourseSkillsAsync(course.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from service in Trackr.Stashed<InMemoryCoachCourseService>()
    from coaches in Trackr.Capture(service.GetCoachesAsync())
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
 Location:                Notes\Y_Async\B_LockTheStableDoors\LockTheStableDoors.cs:32:1
 Original failing run:    11 executions
 Minimal failing case:    4 executions (after 12 shrinks)
 Seed:                    1511313504
 ------------------------------------------------------------
  Executed: Create Coach
   - Trace: Coach Name = Coach-1
 ------------------------------------------------------------
  Executed: Create Course
   - Trace: Course Name = Course-2
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - Trace: Coach Name = Coach-1
   - Trace: Course Name = Course-2
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Input: Course Skills = [ _ ]
   - Trace: Course Name = Course-2
 =====================================================
  !! Spec Failed: Assigned Coaches Must Be Suitable
 =====================================================
 Passed Specs
 - Assigned Coaches Must Be Suitable: 5x
 ------------------------------------------------------------
```
