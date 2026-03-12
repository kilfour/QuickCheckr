## Building a Testing DSL with QuickCheckr
### Model Based Testing

**The Model:**  
```csharp
public class Model
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Sut
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }
    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
```
#### Using Linq

**The Checkr:**  
```csharp
from model in Trackr.Stashed(() => new Model())
from sut in Trackr.Stashed(() => new Sut())
from ops in Checkr.OneOf(
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Add", () =>
    {
        model.Add(input);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_UsingLinq.cs:72:1
 Original failing run:    14 executions
 Minimal failing case:    4 executions (after 13 shrinks)
 Seed:                    1299945702
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 196.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 13x
 ------------------------------------------------------------
```
#### Using Dsl

**The Checkr:**  
```csharp
 Check
    .Model(() => new Model())
    .Sut(() => new Sut())
    .Operation("Add", Fuzzr.Int(),
        (model, sut, a) =>
        {
            model.Add(a);
            sut.Add(a);
        })
    .Operation("Subtract", Fuzzr.Int(),
        (model, sut, a) =>
        {
            model.Subtract(a);
            sut.Subtract(a);
        })
    .Operation("Clear",
        (model, sut) =>
        {
            model.Clear();
            sut.Clear();
        })
    .Observe(
        (model, sut) => model.Result == sut.Result,
        (model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.");
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_UsingDsl.cs:72:1
 Original failing run:    7 executions
 Minimal failing case:    4 executions (after 6 shrinks)
 Seed:                    1879928073
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 193.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 6x
 ------------------------------------------------------------
```
### Going Functional
#### Spike

**The Model:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. FudgeIt(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> FudgeIt(IEnumerable<int> list)
        => list.Where(a => a != 6).Concat(list.Where(a => a == 6));
}
```

**The Invariant:**  
```csharp
Check.ForAll(
    from len in Fuzzr.Int(0, 10)
    from elements in Fuzzr.Int(0, 10).Many(len)
    select elements.ToList(),
    list => list.SequenceEqual(TheReverser.Reverses(TheReverser.Reverses(list))));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                GoingFunctional.cs:27:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    2116954181
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, 6 ]
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Simplest

**The Invariant:**  
```csharp
Check.ForAll(Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Simplest.cs:24:1
 Original failing run:    1 execution
 Seed:                    924452929
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Seeded

**The Invariant:**  
```csharp
Check.ForAll(1948674479, Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_Seeded.cs:26:1
 Original failing run:    1 execution
 Seed:                    1948674479
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Specifying Runs

**The Invariant:**  
```csharp
Check.ForAll(200.Runs(), Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_SpecifyingRuns.cs:24:1
 Original failing run:    1 execution
 Seed:                    823515705
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### With Classifiers

**The Invariant:**  
```csharp
Check.ForAll(Fuzzr.Constant(7), a => a == 6, Try.Default<int>(), Try.Values(6));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_WithClassifiers.cs:24:1
 Original failing run:    1 execution
 Seed:                    1830629706
 ------------------------------------------------------------
  Executed:
   - Input: Input = 7
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### Applies Default Reducers

**The Invariant:**  
```csharp
Check.ForAll(1429680296, Fuzzr.Int(0, 10), a => a < 2);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                F_AppliesDefaultReducers.cs:24:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1429680296
 ------------------------------------------------------------
  Executed:
   - Input: Input = 5
            Redux   2
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### With Classifiers Seeded Showing Redux Too

**The Invariant:**  
```csharp
Check.ForAll(893841622, Fuzzr.Int(0, 10), a => a == 6, Try.Default<int>(), Try.Values(6));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                G_WithClassifiersSeededShowingReduxToo.cs:24:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    893841622
 ------------------------------------------------------------
  Executed:
   - Input: Input = 9
            Redux   7
 ===================================
  !! Expectation Failed: Property
 ===================================
```
#### With Custom Reducer

**The Invariant:**  
```csharp
Check.ForAll(257911700, Fuzzr.Int(0, 10), a => a != 6, Reduce.Values([0, 6]));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H_WithCustomReducer.cs:24:1
 Original failing run:    1 execution
 Seed:                    257911700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
### Web APIs
This is an example from an external lib : 'QuickWebr'.  
# Api
## Main Api Test
```csharp
Webr.Named("WebrTest")
    .Context(() => new CustomWebApplicationFactory())
    .Client(a => a.CreateClient())
    .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
    .Database(a => a.GetDbContext())
    .DisableWarnings()
    .PoolOf<CoachInfo>()
    .PoolOf<CourseInfo>()
    .Methods(
        new CreateCoach(),
        new UpdateCoachSkills(),
        new CreateCourse(),
        new UpdateCourseSkills(),
        new UpdateTimeSlots(),
        new ConfirmCourse(),
        new AssignCoachToCourse())
    .ForAll<CoachInfo>("All Assigned Coaches Must Be Suitable",
        (api, coachInfo) =>
        {
            var coach = api.Query(db => db.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
```
## Coach Methods
```csharp
public class CreateCoach : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Create("/coaches")
            .With(
                from name in Fuzzr.String()
                from email in Fuzzr.String()
                select new RegisterCoachRequest(name, email))
            .Returns<int>(a => a > 0)
            .When<CoachInfo>(a => a.Count <= 3)
            .Store(a => new CoachInfo(a))
            .Load((db, id) => db.Find<Coach>(Id<Coach>.From(id)))
            .Expect(
                ("Name", (request, coach) => coach.Name.Value == request.Name),
                ("Email", (request, coach) => coach.Email.Value == request.Email));
}
```
```csharp
public class UpdateCoachSkills : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/coaches/{id}/skills")
            .When<CoachInfo>(a => a.Count != 0)
            .With(
                from skills in Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(3, 5)
                select new UpdateSkillsRequest([.. skills])
                , coach => Shrink.ValueFromState(coach.Skills, []))
            .Route(a => a.Id, a => $"/coaches/{a}/skills")
            .Store((coach, request) => coach with { Skills = request.Skills })
            .Load((db, id) => db.Find<Coach>(Id<Coach>.From(id)))
            .Expect(
                ("Skills", (request, coach) => Skills.Equal(request.Skills, coach.Skills)));
}
```
## Course Methods
```csharp
public class CreateCourse : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Create("/courses")
            .With(
                from name in Fuzzr.String()
                from startDate in Fuzzr.DateOnly(1.January(2026), 31.December(2026))
                from endDate in Fuzzr.DateOnly(startDate, startDate.AddDays(50))
                select new CreateCourseRequest(name, startDate, endDate))
            .Returns<int>(a => a > 0)
            .When<CourseInfo>(a => a.Count < 3)
            .Store(a => new CourseInfo(a))
            .Load((db, id) => db.Find<Course>(Id<Course>.From(id)))
            .Expect(
                ("Name", (request, course) => course.Name.Value == request.Name),
                ("Start Date", (request, course) => course.Period.Start == request.StartDate),
                ("End Date", (request, course) => course.Period.End == request.EndDate));
}
```
```csharp
public class UpdateCourseSkills : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/skills")
            .When<CourseInfo>(info => !info.IsConfirmed)
            .With(
                from skills in Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(3, 5)
                select skills.ToList())
            .Route(a => a.Id, a => $"/courses/{a}/skills")
            .Load((db, id) => db.Find<Course>(Id<Course>.From(id)))
            .Expect(("Skills", (request, course) => Skills.Equal(request, course.RequiredSkills)));
}
```
```csharp
public class UpdateTimeSlots : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/timeslots")
            .When<CourseInfo>(info => !info.IsConfirmed)
            .With(TimeSlotsFuzzr, Try.Values(new TimeSlotRequest(CourseDay.Monday, 10, 11)))
            .Route(a => a.Id, a => $"/courses/{a}/timeslots")
            .Store((course, request) => course with { HasTimeSlots = true })
            .Load((db, id) => db.Find<Course>(Id<Course>.From(id))!)
            .Expect(("Timeslots", (request, course) => SlotsAreEqual(request, course.TimeSlots)));
    private static readonly FuzzrOf<List<TimeSlotRequest>> TimeSlotsFuzzr =
        from key in Fuzzr.Guid()
        from slots in TimeSlotFuzzr(key).Many(2, 5)
        select slots.ToList();
    private static FuzzrOf<TimeSlotRequest> TimeSlotFuzzr(object key) =>
        from day in Fuzzr.Enum<CourseDay>().Unique(key)
        from start in Fuzzr.Int(9, 17)
        from end in Fuzzr.Int(start + 1, 18)
        select new TimeSlotRequest(day, start, end);
    private static bool SlotsAreEqual(IEnumerable<TimeSlotRequest> expected, IEnumerable<TimeSlot> actual)
    {
        var expectedOrdered = expected.OrderBy(a => (a.Day, a.Start, a.End));
        var actualOrdered =
            actual.Select(a => new TimeSlotRequest(a.Day, a.Start.Value, a.End.Value))
                .OrderBy(a => (a.Day, a.Start, a.End));
        return expectedOrdered.SequenceEqual(actualOrdered);
    }
}
```
```csharp
public class ConfirmCourse : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/confirm")
            .When<CourseInfo>(info => info.HasTimeSlots && !info.IsConfirmed)
            .Route(a => a.Id, a => $"/courses/{a}/confirm")
            .Store((course) => course with { IsConfirmed = true })
            .Load((db, id) => db.Find<Course>(Id<Course>.From(id)))
            .Expect(("Confirmed", (course) => course.IsConfirmed));
}
```
```csharp
public class AssignCoachToCourse : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/assign-coach")
            .When<CourseInfo, CoachInfo>(
                (course, coach) => course.IsConfirmed && IsSuitableFor(api, course, coach))
            .With((course, coach) => new AssignCoachRequest(coach.Id))
            .Route((course, coach) => course.Id, a => $"/courses/{a}/assign-coach")
            .Load((ctx, course, coach) => (ctx.Find<Course>(Id<Course>.From(course.Id)), ctx.Find<Coach>(Id<Coach>.From(coach.Id))))
            .Expect(
                ("Coach is assigned", (course, coach) =>
                    course.AssignedCoach == coach &&
                    coach.AssignedCourses.Contains(course)));
    private static bool IsSuitableFor(IDbAccess db, CourseInfo courseInfo, CoachInfo coachInfo)
    {
        var course = db.Query(ctx => ctx.Find<Course>(Id<Course>.From(courseInfo.Id))!);
        if (course == null) return false;
        if (course.AssignedCoach != null) return false;
        var coach = db.Query(ctx => ctx.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
        if (coach == null) return false;
        return coach.IsSuitableFor(course);
    }
}
```
## Report
```
------------------------------------------------------------
 Original failing run:    35 executions
 Minimal failing case:    8 executions (after 39 shrinks)
 Seed:                    1337846571
 ------------------------------------------------------------
  Executed: Route: /courses
   -  Pool: '/courses' to Pool = CourseInfo-3
   - Input: '/courses' Request = { Name: "fntkpukp", StartDate: 9.August(2026), EndDate: 25.September(2026) }
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/skills
   -  Pool: /courses/{id}/skills           = CourseInfo-3
   - Input: '/courses/{id}/skills' Request = [ "UnitTesting" ]
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/timeslots
   -  Pool: /courses/{id}/timeslots           = CourseInfo-3
   - Input: '/courses/{id}/timeslots' Request = [ _ ]
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/confirm
   -  Pool: /courses/{id}/confirm = CourseInfo-3
 ------------------------------------------------------------
  Executed: Route: /coaches
   -  Pool: '/coaches' to Pool = CoachInfo-3
 ------------------------------------------------------------
  Executed: Route: /coaches/{id}/skills
   -  Pool: /coaches/{id}/skills           = CoachInfo-3
   - Input: '/coaches/{id}/skills' Request = { Skills: [ "UnitTesting" ] }
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/assign-coach
   -  Pool: /courses/{id}/assign-coach from Pool = ( CourseInfo-3, CoachInfo-3 )
 ------------------------------------------------------------
  Executed: Route: /coaches/{id}/skills
   -  Pool: /coaches/{id}/skills           = CoachInfo-3
   - Input: '/coaches/{id}/skills' Request = { Skills: [ "Architecture", "EF Core", "ASP.NET", "Elm", "C#" ] }
            Redux                            { Skills: [ ] }
 ================================================================
  !! Expectation Failed: All Assigned Coaches Must Be Suitable
 ================================================================
 Passed Expectations
 - '/courses' Status Code is Success: 3x
 - '/courses' Response: 3x
 - '/courses' Name: 3x
 - '/courses' Start Date: 3x
 - '/courses' End Date: 3x
 - All Assigned Coaches Must Be Suitable: 34x
 - '/coaches' Status Code is Success: 4x
 - '/coaches' Response: 4x
 - '/coaches' Name: 4x
 - '/coaches' Email: 4x
 - '/courses/{id}/skills' Status Code is Success: 5x
 - '/courses/{id}/skills' Skills: 5x
 - '/courses/{id}/timeslots' Status Code is Success: 3x
 - '/courses/{id}/timeslots' Timeslots: 3x
 - '/coaches/{id}/skills' Status Code is Success: 16x
 - '/coaches/{id}/skills' Skills: 16x
 - '/courses/{id}/confirm' Status Code is Success: 3x
 - '/courses/{id}/confirm' Confirmed: 3x
 - '/courses/{id}/assign-coach' Status Code is Success: 1x
 - '/courses/{id}/assign-coach' Coach is assigned: 1x
 ------------------------------------------------------------
```  
### Explore and Exemplify
#### Bolting Horses Dsl

**The Invariant:**  
```csharp
SystemCheckr
    .Define()
    // Using `.Named("Bolting Horses")` instead of `.Define()` persists the report.
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
#### A Simple Scenario
##### Step One

**The Invariant:**  
```csharp
SystemCheckr
    .Define()
    .PoolOf<Appointment>()
    .Operations(
        new CreateAppointment())
    .Run(100.Runs(), 10.ExecutionsPerRun());
```

**The Fuzzr:**  
```csharp
// Todo: use QuickFuzzr.Evil, maybe add .Remove() or something somehow
Fuzzr.OneOf(
    Fuzzr.String(),
    Fuzzr.Constant(string.Empty),
    Fuzzr.Constant<string>(null!),
    Fuzzr.Constant("  "));
```

**The Operation:**  
```csharp
public class CreateAppointment : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Create<Appointment>()
            .When(coaches => coaches.Count <= 10)
            .With(EvilString())
            .Perform(input => new Appointment(input))
            .Expect(("Name", (appointment, input) => appointment.Title == input));
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_StepOne.cs:51:1
 Original failing run:    1 execution
 Seed:                    1279955743
 ------------------------------------------------------------
  Executed: Create Appointment
   -  Pool: 'Create Appointment' to Pool = Appointment-1
   - Input: 'Create Appointment' Input   = null
 ===========================================================================
  !! System.ArgumentNullException: Title cannot be null or whitespace (Parameter 'title')
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.Appointment.EditTitle(String title) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\Appointment.cs:line 18
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.Appointment..ctor(String title) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\Appointment.cs:line 10
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.A_StepOne.CreateAppointment.<>c.<Define>b__0_1(String input) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\A_StepOne.cs:line 52
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl.OperationBuilders.CreateBuilder3`2.<>c__DisplayClass7_1.<GetCheckr>b__8() in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\Dsl\OperationBuilders\CreateBuilder.cs:line 45
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
##### Step Two

**The Fuzzr:**  
```csharp
Fuzzr.OneOf(
    Fuzzr.String(),
    Fuzzr.Constant(string.Empty),
    Fuzzr.Constant("  "));
```

**The Operation:**  
```csharp
public class CreateAppointment : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Create<Appointment>()
            .When(coaches => coaches.Count <= 10)
            .With(EvilString())
            .Perform(input => new Appointment(input))
            .Example("Empty string", string.Empty, result => result.ThrewExactly<ArgumentNullException>())
            .Expect(("Name", (appointment, input) => appointment.Title == input));
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_StepTwo.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1941262939
 ------------------------------------------------------------
  Executed: 'Create Appointment' Empty string, Create Appointment
   -  Pool: 'Create Appointment' to Pool = Appointment-1
   - Input: 'Create Appointment' Input   = "  "
 ===========================================================================
  !! System.ArgumentNullException: Title cannot be null or whitespace (Parameter 'title')
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.Appointment.EditTitle(String title) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\Appointment.cs:line 18
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.Appointment..ctor(String title) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\Appointment.cs:line 10
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.B_StepTwo.CreateAppointment.<>c.<Define>b__0_1(String input) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\B_StepTwo.cs:line 50
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl.OperationBuilders.CreateBuilder3`2.<>c__DisplayClass7_1.<GetCheckr>b__8() in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\Dsl\OperationBuilders\CreateBuilder.cs:line 45
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
 Passed Expectations
 - 'Create Appointment' Empty string: 1x
 ------------------------------------------------------------
```
##### Step Three

**The Fuzzr:**  
```csharp
Fuzzr.OneOf(
    Fuzzr.String(),
    Fuzzr.Constant(string.Empty));
```

**The Operation:**  
```csharp
public class CreateAppointment : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Create<Appointment>()
            .When(coaches => coaches.Count <= 10)
            .With(EvilString())
            .Perform(input => new Appointment(input))
            .Example("Null string", null!, result => result.ThrewExactly<ArgumentNullException>())
            .Example("Whitespace string", " ", result => result.ThrewExactly<ArgumentNullException>())
            .Expect(("Name", (appointment, input) => appointment.Title == input));
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_StepThree.cs:48:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1493980658
 ------------------------------------------------------------
  Executed: 'Create Appointment' Null string, 'Create Appointment' Whitespace string, Create Appointment
   -  Pool: 'Create Appointment' to Pool = Appointment-2
   - Input: 'Create Appointment' Input   = ""
 ===========================================================================
  !! System.ArgumentNullException: Title cannot be null or whitespace (Parameter 'title')
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.Appointment.EditTitle(String title) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\Appointment.cs:line 18
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.Appointment..ctor(String title) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\Appointment.cs:line 10
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub.C_StepThree.CreateAppointment.<>c.<Define>b__0_1(String input) in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\B_ASimpleScenario\Sub\C_StepThree.cs:line 49
   at QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl.OperationBuilders.CreateBuilder3`2.<>c__DisplayClass7_1.<GetCheckr>b__8() in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\D_DslBuilding\X_Dsl\Dsl\OperationBuilders\CreateBuilder.cs:line 45
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
 Passed Expectations
 - 'Create Appointment' Name: 1x
 - 'Create Appointment' Null string: 1x
 - 'Create Appointment' Whitespace string: 1x
 ------------------------------------------------------------
```
##### Step Four

**The Operation:**  
```csharp
public class CreateAppointment : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Create<Appointment>()
            .When(coaches => coaches.Count <= 10)
            .With(Fuzzr.String())
            .Perform(input => new Appointment(input))
            .Example("Empty string", string.Empty, result => result.ThrewExactly<ArgumentNullException>())
            .Example("Null string", null!, result => result.ThrewExactly<ArgumentNullException>())
            .Example("Whitespace string", " ", result => result.ThrewExactly<ArgumentNullException>())
            .Expect(("Name", (appointment, input) => appointment.Title == input));
}
```

**The Report:**  
```text
1 Run
 Passed Expectations
 - 'Create Appointment' Name: 10x
 - 'Create Appointment' Empty string: 1x
 - 'Create Appointment' Null string: 1x
 - 'Create Appointment' Whitespace string: 1x
 ------------------------------------------------------------
```
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
  
We already saw in the chapter on [Configuration][Configuration] how you can turn some knobs to customize QuickCheckr's behaviour
and/or reporting globally.  
But there are ways to obtain more fine-grained control. That is what this chapter is about.  
### Contents

- [Showr][A_TheShowrClass]
- [Custom Shrinkers][CustomShrinkers]
  

[Configuration]: #configuration

[A_TheShowrClass]: #the-showr-class

[CustomShrinkers]: #custom-shrinkers
### The Showr Class
This static factory class produces a CheckrOf<Case> which allows one to configure the pretty-printer used in the report.

It is implemented as such, instead of as an option on the configuration object,
because this allows for two useful things:  
> You can change configuration anywhere in the `LINQ chain`, multiple times.  

**The Checkr:**  
```csharp
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from showrOne in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from traceOne in Checkr.Trace("One", () => anInt)
from showrTwo in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"OTHER {a}"))
from traceTwo in Checkr.Trace("Two", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: One   = PREFIXED 7
   - Trace: Two   = OTHER 7
```
> Configuration can depend on context.  

**The Model:**  
```csharp
public class Product
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record OrderRequest(Guid ProductId, int Amount);

public class Warehouse(List<Product> productsForSale)
{
    public List<Product> ProductsForSale => productsForSale;

    public void PlaceOrder(OrderRequest request) =>
        productsForSale.First(a => a.Id == request.ProductId).AmountInStock -= request.Amount;
}
```

**The Checkr:**  
```csharp
from warehouse in Trackr.Stashed(() => new Warehouse(ProductsForSale()))
from showr in Showr.ForInput(() =>
    Please.AllowMe().ToReplace<OrderRequest>(a =>
        $"Ordered {a.Amount} pieces of {warehouse.ProductsForSale.Single(b => a.ProductId == b.Id).Name}."))
from request in Checkr.Input("Order request", RequestFuzzr(warehouse))
from placeOrder in Checkr.Act("Place order", () => warehouse.PlaceOrder(request))
from noNegativeStock in Checkr.Expect("No negative stock",
    () => warehouse.ProductsForSale.Select(a => a.AmountInStock).All(a => a >= 0))
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed: Place order
   - Input: Order request = { ProductId: b1c88a3c-444e-4776-a5c4-c660a75e9365, Amount: 53 }
                            Ordered 53 pieces of Product One.
```
A full reference to the available options can be found [here][A_ShowrReference].  

[A_ShowrReference]: #showr-reference
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
### Custom Reducers Old

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
//from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.TowardsZero)
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType(Reduce.TowardsZero))
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
#### Nested In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from answer in Checkr.Input("Answer", Fuzzr.Int(), Reduce.TowardsZero)
from universe in Checkr.Capture(() => new TheUniverse { Answer = answer })
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1930190827
 ------------------------------------------------------------
  Executed:
   - Input: Answer = 96
            Redux    43
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
#### Nested Reducer In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(), Reduce.TowardsZero)
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedReducerInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    686899245
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { Answer: 61 }
            Redux      { Answer: 43 }
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
#### Nested Reducer In Object On Path

**The Model:**  
```csharp
public class TheUniverse
{
    public int AnswerOne { get; set; }
    public int AnswerTwo { get; set; }
    public int AnswerThree { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(),
    [("Universe.AnswerOne", Reduce.TowardsZero), ("Universe.AnswerTwo", Reduce.Towards(100))])
from trace in Checkr.Trace("Universe", () => universe)
from justRight in Checkr.Expect("just right", () => universe.AnswerOne <= 42 || universe.AnswerTwo >= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedReducerInObjectOnPath.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1118829238
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { AnswerOne: 91, AnswerTwo: 15 }
            Redux      { AnswerOne: 43, AnswerTwo: 41 }
   - Trace: Universe = { AnswerOne: 91, AnswerTwo: 15, AnswerThree: 52 }
 =====================================
  !! Expectation Failed: just right
 =====================================
```
#### Custom

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), CustomTowardsZero.Reducer())
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Reducer:**  
```csharp
public class CustomTowardsZero
{
    /// <summary>
    /// Returns a reducer which tries to reduce the input to zero.
    /// </summary>
    /// <remarks>
    /// The reducer will try to reduce the value by one if it is greater than zero,
    /// and increase the value by one if it is less than zero.
    /// </remarks>
    public static Func<Reducer<int>> Reducer() =>
        () => new CustomTowardsZero().Reduce;
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
#### Reduction Not Possible
What to do if all reduction values fail ?  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.TowardsZero)
from expect in Checkr.Expect("Input should be greater than 42", () => input > 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReductionNotPossible.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1373577700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 21
            Redux   0
 ==========================================================
  !! Expectation Failed: Input should be greater than 42
 ==========================================================
```
### Custom Shrinkers

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
As explained in [OnShrinking][InputShrinking], a Shrinker is comprised of:
  1. A *[Classifier][Classifiers]* : responsible for finding the relevant inputs.
  2. A *[Reducer][Reducers]* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these on a per-input basis.  
This gives you precise control over how shrinking behaves when the default strategies are insufficient for your domain.  

By default, built-in classifiers and reducers are provided for common types.  
Custom shrinkers are intended for cases where domain knowledge can guide shrinking more effectively.  

  

[InputShrinking]: #input-shrinking

[Classifiers]: #classifiers

[Reducers]: #reducers
#### Classifiers
Classification is done by trying different *candidates* for an input and verifying if the test still fails.  

- If removing/changing the input (via candidates) still reproduces the failure, the input is irrelevant.
- If the failure goes away for at least one candidate, the input is relevant.

So when supplying a custom classifier, you are basically supplying a list of candidates.  
There are various ways to do this :
- [Candidate-based Classifiers][CandidateClassifiers]
- [Policy-based Classifiers][PolicyBasedClassifiers]
  
The following example demonstrates how the various classifiers can be used chained.    
It is a bit of a contrived example as the default classifiers handle this case just fine.  

**The Checkr:**  
```csharp
from answer in Checkr.Input("answer", Fuzzr.Int(), Try.Default<int>(), Try.Values(0, 1, 42))
from expect in Checkr.Expect("answer == 42", () => answer == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Classifiers.cs:37:1
 Original failing run:    1 execution
 Seed:                    1521363987
 ------------------------------------------------------------
  Executed:
   - Input: answer = 63
 =======================================
  !! Expectation Failed: answer == 42
 =======================================
```

[CandidateClassifiers]: #candidate-based-classifiers

[PolicyBasedClassifiers]: #policy-based-classifiers
##### Candidate-based Classifiers
Here is an overview of all the different ways you can define *candidates* for classification.  
Using a more relevant example this time (where possible), that allows you to deal with [this][E_CheckingNulls] relatively common issue.  
###### Values

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Try.Values(new Thing()))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_TryValues.cs:73:1
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
###### Collection
This is very similar to `Try.Values`.  
It only exists to avoid generic type soup and easily made mistakes when typing candidates supply for collections.  

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(2, 5), Try.Collection([0], [1], [42]))
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_TryCollection.cs:56:1
 Original failing run:    1 execution
 Seed:                    899182715
 ------------------------------------------------------------
  Executed:
   - Input: answers = [ 2, 80 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```
###### Function
This method allows you to supply dynamic candidates using a function.  
The lambda argument of the supplied function, is the original Input value.
  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Try.Function((Thing a) => [new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_TryFunction.cs:73:1
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
###### Fuzzr

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!), Try.Fuzzr(Fuzzr.One<Thing>(), 2))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_TryFuzzr.cs:73:1
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

[E_CheckingNulls]: #checking-nulls
##### Policy-based Classifiers
  
###### Keep
`Try.Keep` explicitly marks an execution as relevant and worth preserving during shrinking.
In most scenarios this is not strictly necessary, as QuickCheckr will naturally retain behaviour that contributes to failure.
`Try.Keep` mainly exists for completeness and for rare performance-tuning cases where you want to prevent the shrinker from exploring removals you already know are pointless.
It also serves as an explicit signal of intent: this step is considered essential evidence.  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Try.Keep<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_TryKeep.cs:46:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Null Thing = null
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
###### None

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Try.None<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```
`Try.None` is a custom classifier that explicitly marks an execution as irrelevant and therefore not worth shrinking.
It is primarily a performance tool: when a branch is known not to influence the outcome,
`Try.None` prevents QuickCheckr from wasting time shrinking or re-executing it. 
This can significantly speed up scenarios that involve expensive operations such as WebAPI calls.
It can also be used to suppress unnecessary "all inputs irrelevant" warnings when that irrelevance is intentional and understood.
  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_TryNone.cs:46:1
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
###### Default
`Try.Keep` applies QuickCheckr's normal relevance detection as one strategy within a classifier chain.
Classifiers are executed in succession, so Try.Default can be placed anywhere in the sequence.
If it finds a relevant candidate, the chain stops. Otherwise, the next classifier is attempted.
This makes it possible to combine engine-driven relevance detection with more opinionated strategies such as Try.Values(...)
without hard-coding one as the global default.  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Try.Default<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_TryDefault.cs:46:1
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
#### Reducers
