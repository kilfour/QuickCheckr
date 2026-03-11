# Bolting Horses Dsl

**The Invariant:**  
```csharp
SystemCheckr
    .Named("Bolting Horses")
    .DisableWarnings()
    .PoolOf<Coach>()
    .PoolOf<Course>()
    .Operations(
        new CreateCoach(),
        new UpdateCoachSkills(),
        new CreateCourse(),
        new UpdateCourseSkills(),
        new AssignCoachToCourse())
    .ForAll<Coach>("All Assigned Coaches Must Be Suitable",
        coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
    .Run(1473455315, 20.ExecutionsPerRun());
```
```csharp
public class CreateCoach : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Create<Coach>()
            .When(coaches => coaches.Count <= 10)
            .With(from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
            .Perform(input => new Coach(input))
            .Expect(("Name", (coach, input) => coach.Name == input));
}
```
```csharp
public class UpdateCoachSkills : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Update<Coach>("Skills")
            .When(coaches => coaches.Count != 0)
            .With(Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            .Perform((coach, input) => coach.UpdateSkills(input))
            .Expect(("Skills", (coach, input) =>
                coach.Skills.Count == input.Count() && coach.Skills.All(input.Contains)));
}
```
```csharp
public class CreateCourse : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Create<Course>()
            .When(courses => courses.Count <= 5)
            .With(from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
            .Perform(input => new Course(input))
            .Expect(("Name", (course, input) => course.Name == input));
}
```
```csharp
public class UpdateCourseSkills : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Update<Course>("Skills")
            .When(courses => courses.Count != 0)
            .With(Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            .Perform((course, input) => course.UpdateRequiredSkills(input))
            .Expect(("Skills", (course, input) =>
                course.RequiredSkills.Count == input.Count() && course.RequiredSkills.All(input.Contains)));
}
```
```csharp
public class AssignCoachToCourse : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Update<Coach, Course>("Assign")
            .When((coach, course) => coach.IsSuitableFor(course))
            .Perform((coach, course) => course.AssignCoach(coach))
            .Expect(("Assigned", (coach, course) => course.AssignedCoach == coach));
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                BoltingHorsesDsl.cs:53:1
 Original failing run:    13 executions
 Minimal failing case:    4 executions (after 14 shrinks)
 Seed:                    1473455315
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: 'Create Course' to Pool = Course-1
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: 'Create Coach' to Pool = Coach-1
 ------------------------------------------------------------
  Executed: Update (Coach, Course) Assign
   -  Pool: Update (Coach, Course) Assign = ( Coach-1, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Update Course Skills         = Course-1
   - Input: 'Update Course Skills' Input = [ _ ]
 ================================================================
  !! Expectation Failed: All Assigned Coaches Must Be Suitable
 ================================================================
 Passed Expectations
 - 'Create Course' Name: 4x
 - All Assigned Coaches Must Be Suitable: 12x
 - 'Create Coach' Name: 4x
 - 'Update Coach Skills' Skills: 2x
 - 'Update (Coach, Course) Assign' Assigned: 2x
 - 'Update Course Skills' Skills: 1x
 ------------------------------------------------------------
```
