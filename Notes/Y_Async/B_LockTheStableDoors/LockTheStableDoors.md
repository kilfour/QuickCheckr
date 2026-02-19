# Lock The Stable Doors
**The Checkr:**  
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from service in Trackr.Stashed(() => new InMemoryCoachCourseService())
    from coaches in Trackr.PoolFor<CoachBy>()
    from courses in Trackr.PoolFor<CourseBy>()
    from dbCourses in Checkr.Capture(service.GetCoursesAsync())
    from operations in Checkr.OneOfWhen(
        Trackr.PoolWhen<CoachBy>(a => a.Count() < 10, CreateCoach),
        Trackr.PoolWhen<CoachBy>(a => a.Any(), UpdateCoachSkills),
        Trackr.PoolWhen<CourseBy>(a => a.Count() < 5, CreateCourse),
        Trackr.PoolWhen<CourseBy>(a => a.Any(), UpdateCourseSkills),
        Trackr.PoolWhen<CoachBy>(a => a.Any() && dbCourses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
    from invariant in AssignedCoachesMustBeSuitable
    select Case.Closed;
```
**Create Coach Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from coach in Checkr.Act("Create Coach", service.CreateCoachAsync(name))
    from toPool in Trackr.ToPool("Store Coach", new CoachBy(name))
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCoachSkills =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from pool in Trackr.OneOfPool<CoachBy>("Coach to update", coach =>
        from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => service.UpdateCoachSkillsAsync(coach.Value.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Create Course Checkr:**  
```csharp
private static readonly CheckrOf<Case> CreateCourse =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from name in Checkr.Input("Course Name",
        from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
    from course in Checkr.Act("Create Course", service.CreateCourseAsync(name))
    from toPool in Trackr.ToPool("Store Course", new CourseBy(name))
    select Case.Closed;
```
**Update Course Skills Checkr:**  
```csharp
private static readonly CheckrOf<Case> UpdateCourseSkills =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from pool in Trackr.OneOfPool<CourseBy>("Course to update", course =>
        from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Course Skills",
            service.UpdateCourseSkillsAsync(course.Value.Name, skills))
        select Case.Closed)
    select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from service in Trackr.GetStashed<InMemoryCoachCourseService>()
    from coaches in Checkr.Capture(service.GetCoachesAsync())
    from assignedCheck in Checkr.ExpectWhen("Assigned Coaches Must Be Suitable",
        () => coaches.Any(coach => coach.AssignedCourses.Any()),
        () => coaches.All(
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course))))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\Y_Async\B_LockTheStableDoors\LockTheStableDoors.cs:31:1
 Original failing run:    13 executions
 Minimal failing case:    4 executions (after 14 shrinks)
 Seed:                    1511313504
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = CoachBy-2
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: Store Course = CourseBy-2
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Assign Coach to Course
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to update = CourseBy-2
   - Input: Course Skills    = [ _ ]
 ============================================================
  !! Expectation Failed: Assigned Coaches Must Be Suitable
 ============================================================
 Passed Expectations
 - Assigned Coaches Must Be Suitable: 5x
 ------------------------------------------------------------
```
