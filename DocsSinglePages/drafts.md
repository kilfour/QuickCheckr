## Building a Testing DSL with QuickCheckr
### Model Based Testing

**The Model:**  
```csharp
public class CalculatorModel
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Calculator
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
from model in Trackr.Stashed(() => new CalculatorModel())
from sut in Trackr.Stashed(() => new Calculator())
from ops in Checkr.OneOf(
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Add", () =>
    {
        model.Add(input);
        sut.Add(input);
    })
    select Case.Closed,
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Subtract", () =>
    {
        model.Subtract(input);
        sut.Subtract(input);
    })
    select Case.Closed,
    Checkr.Act("Clear", () =>
    {
        model.Clear();
        sut.Clear();
    })
)
from observe in Checkr.Expect("Observe", () => model.Result == sut.Result)
from trace in Checkr.TraceWhen("Observation", () => model.Result != sut.Result, () => $"Model: {model.Result}, Sut: {sut.Result}.")
select Case.Closed;
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
    .Model(() => new CalculatorModel())
    .Sut(() => new Calculator())
    .Operation("Add", Fuzzr.Int(),
        (model, a) => model.Add(a),
        (sut, a) => sut.Add(a))
    .Operation("Subtract", Fuzzr.Int(),
        (model, a) => model.Subtract(a),
        (sut, a) => sut.Subtract(a))
    .Operation("Clear",
        model => model.Clear(),
        sut => sut.Clear())
    .Report((model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.")
    .Observe((model, sut) => model.Result == sut.Result);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_UsingDsl.cs:50:1
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
The `Check.` spike version has been removed.  
Currently in the progress of writing a new DSL 'Testr' by adapting the jqwik challenges from LINQ to Testr.  
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
            return coach.AssignedCourses.All(course => coach.IsSuitableFor(course));
        })
    .Run(20.Runs(), 50.ExecutionsPerRun());
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
            .Load((db, info) => db.Find<Coach>(Id<Coach>.From(info.Id)))
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
            .Load((db, info) => db.Find<Course>(Id<Course>.From(info.Id)))
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
 Original failing run:    32 executions
 Minimal failing case:    8 executions (after 39 shrinks)
 Seed:                    1854160474
 ------------------------------------------------------------
  Executed: Auth, Route: /courses
   -  Pool: '/courses' to Pool = CourseInfo-1
   - Input: '/courses' Request = { Name: "nxlarowge", StartDate: 15.March(2026), EndDate: 23.April(2026) }
 ------------------------------------------------------------
  Executed: Route: /coaches
   -  Pool: '/coaches' to Pool = CoachInfo-1
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/skills
   -  Pool: /courses/{id}/skills           = CourseInfo-1
   - Input: '/courses/{id}/skills' Request = [ "JavaScript" ]
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/timeslots
   -  Pool: /courses/{id}/timeslots           = CourseInfo-1
   - Input: '/courses/{id}/timeslots' Request = [ _ ]
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/confirm
   -  Pool: /courses/{id}/confirm = CourseInfo-1
 ------------------------------------------------------------
  Executed: Route: /coaches/{id}/skills
   -  Pool: /coaches/{id}/skills           = CoachInfo-1
   - Input: '/coaches/{id}/skills' Request = { Skills: [ "JavaScript" ] }
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/assign-coach
   -  Pool: /courses/{id}/assign-coach from Pool = ( CourseInfo-1, CoachInfo-1 )
 ------------------------------------------------------------
  Executed: Route: /coaches/{id}/skills
   -  Pool: /coaches/{id}/skills           = CoachInfo-1
   - Input: '/coaches/{id}/skills' Request = { Skills: [ "C#", "React", "CI/CD", "DomainDrivenDesign", "EF Core" ] }
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
 - All Assigned Coaches Must Be Suitable: 31x
 - '/coaches' Status Code is Success: 4x
 - '/coaches' Response: 4x
 - '/coaches' Name: 4x
 - '/coaches' Email: 4x
 - '/courses/{id}/timeslots' Status Code is Success: 6x
 - '/courses/{id}/timeslots' Timeslots: 6x
 - '/courses/{id}/skills' Status Code is Success: 3x
 - '/courses/{id}/skills' Skills: 3x
 - '/coaches/{id}/skills' Status Code is Success: 11x
 - '/coaches/{id}/skills' Skills: 11x
 - '/courses/{id}/confirm' Status Code is Success: 3x
 - '/courses/{id}/confirm' Confirmed: 3x
 - '/courses/{id}/assign-coach' Status Code is Success: 2x
 - '/courses/{id}/assign-coach' Coach is assigned: 2x
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
## On Shrinking
### Philosophy

QuickCheckr keeps shrinking simple and predictable.  
It never tries to *outsmart* your domain with clever global algorithms.  
Shrinking is purely local and irrelevance-based: if removing a value (or execution) still makes the property fail,
QuickCheckr marks it as *irrelevant*.  
This avoids surprising or *magical* shrink results.  
### Contents

- [Execution Shrinking][ExecutionShrinking]
- [Input Shrinking][InputShrinking]
- [Input Reduction][InputReduction]
- [Deliberation Shrinking][DeliberationShrinking]
  

[ExecutionShrinking]: #execution-shrinking

[InputShrinking]: #input-shrinking

[InputReduction]: #input-reduction

[DeliberationShrinking]: #deliberation-shrinking
### Execution Shrinking
[On Shrinking][OnShrinking] | [Next][InputShrinking]  
  
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

[OnShrinking]: #on-shrinking

[InputShrinking]: #input-shrinking
### Input Shrinking
[On Shrinking][OnShrinking] | [Previous][ExecutionShrinking] | [Next][InputReduction]  
  
This is the next step in QuickCheckr shrinking and the most complex one.  

In QuickCheckr, **input shrinking is a two-step process**:

1. **Classification**: decide *what is relevant* for reproducing the failure.
2. **Reduction (Redux)**: simplify the *relevant* parts to improve human understanding, while still reproducing the failure.

A key principle: the engine always keeps the **original failing input**.
Reduction produces a **Redux view** alongside the original.
The report can therefore show both **Original** and **Redux** without pretending the original counterexample was replaced.  
Reduction is (by default) an opt-in feature. In this chapter we are mainly demonstrating Classification.
Reduction will be elaborated on later.
  
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

[OnShrinking]: #on-shrinking

[ExecutionShrinking]: #execution-shrinking

[InputReduction]: #input-reduction
### Input Reduction
[On Shrinking][OnShrinking] | [Previous][InputShrinking] | [Next][DeliberationShrinking]  
  
After InputShrinking has classified an input as either relevant or irrelevant, any registered *reducers*
are run on a relevant input (example below).  

Note the use of the word *registered*. Staying true to it's shrinking philosophy,
QuickChecker does not try and guess how to reduce input values. It's up to the user to supply that information.  

Most of the time, even in the example here I think, reducers are not needed to figure out what went wrong from the report.  
But in some cases it is a must.  

So what is a reducer ?  

Well let me show you:  

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
from amount in Checkr.Input("Amount", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
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
  

[OnShrinking]: #on-shrinking

[InputShrinking]: #input-shrinking

[DeliberationShrinking]: #deliberation-shrinking
### Deliberation Shrinking
[On Shrinking][OnShrinking] | [Previous][InputReduction]  
  
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
from expect in Checkr.ExpectWhen("List count > 2 and does not contain 1",
    () => input.Count() > 2,
    () => !input.Contains(1))
select Case.Closed;
```
Now when the list contains multiple elements `1`, we might get:  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedy
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:45:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    227109555
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, _ ]
   - Trace: List  = [ 1, 2, 1 ]
 ================================================================
  !! Expectation Failed: List count > 2 and does not contain 1
 ================================================================
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

[OnShrinking]: #on-shrinking

[InputReduction]: #input-reduction
## You Can Go Your Own Way
We already saw in the chapter on [Configuration][Configuration] how you can turn some knobs to customize QuickCheckr's behaviour
and/or reporting globally.  
But there are ways to obtain more fine-grained control. That is what this chapter is about.  
### Contents

- [Custom Shrinkers][CustomShrinkers]
- [Showr][A_TheShowrClass]
  

[Configuration]: #configuration

[A_TheShowrClass]: #the-showr-class

[CustomShrinkers]: #custom-shrinkers
### Custom Shrinkers
As explained in [OnShrinking][InputShrinking], a Shrinker is comprised of:
  1. A *[Classifier][Classifiers]* : responsible for finding the relevant inputs.
  2. A *[Reducer][Reducers]* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these on a per-input basis.  
This gives you precise control over how shrinking behaves when the default strategies are insufficient for your domain.  

By default, built-in classifiers and reducers are provided for common types.  
Custom shrinkers are intended for cases where domain knowledge can guide shrinking more effectively. 

They are registered on a per-input basis using an overload on the `Checkr.Input)()` method.  
Contruction of a shrinker is done using factory methods on the `Shrink` static class.

Let's have a look at ...  
#### A Simple Example

**The Checkr:**  
```csharp
from answers in Checkr.Input("Answer", Fuzzr.Int(),
    Shrink.OnType<int>(
        s => s.Classify(Try.Values(42)),
        s => s.Simplify(Reduce.TowardsZero)))
from expect in Checkr.Expect("Answer equals 42", () => answers == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_FirstShrinker.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1895340861
 ------------------------------------------------------------
  Executed:
   - Input: Answer = 53
            Redux    43
 ===========================================
  !! Expectation Failed: Answer equals 42
 ===========================================
```
##### Explaining the Example, TODO: REVIEW THIS TEST/DOC
The example above is intentionally simple. The expectation only passes for the value `42`,
which makes it easy to see how the custom shrinker influences the reported case.

QuickCheckr already provides built-in classifiers and reducers for common types such as `int`.
These default strategies work well in most situations, but they are necessarily generic: 
they do not know which values are meaningful in the context of a specific check.

By adding:

```csharp
s => s.Classify(Try.Values(42))
```

we tell QuickCheckr that `42` is a value worth trying explicitly during shrinking.

The reducer then continues simplifying the remaining failing value:

```csharp
s => s.Simplify(Reduce.TowardsZero)
```

In the report, the original failing value is `53`, while the final reported value becomes `43`.
This is useful because `43` is the closest failing value above the only passing value (`42`).

Although the expectation itself is artificial, the example illustrates the purpose of custom shrinkers:
they allow shrinking to be guided by values that are semantically meaningful for the domain, not just structurally smaller.
  
#### A Second Example

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(1, 5),
    Shrink.OnType<int>(
        s => s.Classify(Try.Values(42)),
        s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("original", () => answers)
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_SecondShrinker.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    960262083
 ------------------------------------------------------------
  Executed:
   - Input: answers  = [ 73 ]
            Redux      [ 43 ]
   - Trace: original = [ 73 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```

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
from answer in Checkr.Input("answer", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Classify(Try.Default<int>(), Try.Values(0, 1, 42))))
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
    Shrink.OnType<Thing>(
        s => s.Classify(Try.Values(new Thing()))))
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
from answers in Checkr.Input("answers", Fuzzr.Int().Many(2, 5),
    Shrink.OnType<IEnumerable<int>>(s => s.Classify(Try.Collection([0], [1], [42]))))
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
    Shrink.OnType<Thing>(s => s.Classify(Try.Function((Thing a) => [new Thing()]))))
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
from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(s => s.Classify(Try.Fuzzr(Fuzzr.One<Thing>(), 2))))
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
    Shrink.OnType<Thing>(s => s.Classify(Try.Keep<Thing>())))
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
    Shrink.OnType<Thing>(s => s.Classify(Try.Default<Thing>())))
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
##### Custom Reducers Old
Reducers are a relatively new addition to the QuickCheckr arsenal.  
And you could, rightfully so, point out that they violate it's core philosophy
of not rewriting what actually happened.  

But once again, it is a feature born out of necessity.  
 
With that caveat out of the way, let's dig in.  
  
###### Values Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 2, 1, 0]))))
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
###### Values Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 0, 1, 2]))))
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
###### Values Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 0, 1, 2], true))))
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
###### Collection
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5),
   Shrink.OnType<IEnumerable<int>>(s => s.Simplify(Reduce.Collection([[3], [2], [1], [0]]))))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_Collection.cs:32:1
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
###### Collection Of
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5),
   Shrink.OnType<IEnumerable<int>>(s => s.Simplify(Reduce.CollectionOf<int>(a => [[3], [2], [1], [0]]))))
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
###### Function Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Function<int>(a => [3, 2, 1, 0]))))
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
###### Function Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Function<int>(a => [3, 0, 1, 2]))))
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
###### Function Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Function<int>(a => [3, 0, 1, 2], true))))
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
###### Chained

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(5), Reduce.Towards(0))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Chained.cs:32:1
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
###### Reduce Towards Zero

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
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
###### Reduce Minimize String

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(),
    Shrink.OnType<string>(s => s.Simplify(Reduce.MinimizeString)))
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
###### Nested In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from answer in Checkr.Input("Answer", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
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
  
  
###### Nested Reducer In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(), Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
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
  
  
###### Custom

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(CustomTowardsZero.Reducer())))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Reducer:**  
```csharp
public class CustomTowardsZero
{
    public static Func<Reducer<int>> Reducer() =>
        () => new CustomTowardsZero().Reduce;
    private int lastFailed = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var value = result.ValueAs<int>();
        if (result.Failed)
            lastFailed = value;
        else
            return new Reduction.Stop(lastFailed);
        if (value == 0)
            return new Reduction.Stop(lastFailed);
        if (value > 0)
            value--;
        else
            value++;
        return new Reduction.Next(value);
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
###### Reduction Not Possible

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from expect in Checkr.Expect("Input should be greater than 42", () => input > 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReductionNotPossible.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1373577700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 21
            Redux   0
   - WARNING: No witness for reducer at 'Input'.
 ==========================================================
  !! Expectation Failed: Input should be greater than 42
 ==========================================================
```
##### On Path

**The Checkr:**  
```csharp
from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 21)),
    Shrink.When<int>(
        s => s.As<Inter>(a => a.A).Target(),
        s => s.Simplify(Reduce.Towards(3))),
    Shrink.When<int>(
        s => s.As<Inter>(a => a.B).Target(),
        s => s.Simplify(Reduce.Towards(0))))
from expect in Checkr.Expect("Sum smaller than 11.", () => inter.A + inter.B < 11)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                OnPath.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    776489069
 ------------------------------------------------------------
  Executed:
   - Input: Inter = { A: 42, B: 21 }
            Redux   { A: 3, B: 8 }
   - WARNING: No witness for reducer at 'Inter.A'.
 ===============================================
  !! Expectation Failed: Sum smaller than 11.
 ===============================================
```
##### On Specific Value

**The Checkr:**  
```csharp
from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 5)),
    Shrink.When<int>(
        s => s.Value(a => a == 5),
        s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("Original", () => inter)
from expect in Checkr.Expect("All ints smaller than 1.", () => inter.A < 1 && inter.B < 1)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                OnSpecificValue.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1329962298
 ------------------------------------------------------------
  Executed:
   - Input: Inter    = { B: 5 }
            Redux      { B: 1 }
   - Trace: Original = { A: 42, B: 5 }
 ===================================================
  !! Expectation Failed: All ints smaller than 1.
 ===================================================
```
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
#### For Classification
##### Default

**The Checkr:**  
```csharp
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 42 }, R = new Num { Value = 0 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Default.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    844205832
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = { Op: Div, R: { Value: 0 } }
   - Trace: Original = { Op: Div, L: { Value: 42 }, R: { Value: 0 } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
##### Customized

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 84 }, R = new Num { Value = 2 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_Customized.cs:32:1
 Original failing run:    1 execution
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( 84 / 2 )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Value: 2 } }
 =================================
  !! Expectation Failed: Not 42
 =================================
```
##### Customized Nested

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term
    {
        L = new Num { Value = 84 },
        R = new Term { L = new Num { Value = 1 }, R = new Num { Value = 1 }, Op = Operation.Add },
        Op = Operation.Div
    }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_CustomizedNested.cs:32:1
 Original failing run:    1 execution
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( 84 / ( 1 + 1 ) )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Op: Add, L: { Value: 1 }, R: { Value: 1 } } }
 =================================
  !! Expectation Failed: Not 42
 =================================
```
##### Customized With Irrelevant

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 84 }, R = new Num { Value = 0 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_CustomizedWithIrrelevant.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( _ / 0 )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Value: 0 } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
##### Customized With Reducer

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term
    {
        L = new Num { Value = 84 },
        R = new Term { L = new Num { Value = 10 }, R = new Num { Value = 20 }, Op = Operation.Div },
        Op = Operation.Div
    }),
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                E_CustomizedWithReducer.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( _ / ( 10 / 20 ) )
            Redux      ( _ / ( 1 / 2 ) )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Op: Div, L: { Value: 10 }, R: { Value: 20 } } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
 Passed Expectations
 - Not 42: 1x
 ------------------------------------------------------------
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
#### Nullable Types

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Constant<Nullable<int>>(null!))
from expect in Checkr.Expect("Fails", () => input.HasValue)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_NullableTypes.cs:32:1
 Original failing run:    1 execution
 Seed:                    393010076
 ------------------------------------------------------------
  Executed:
   - Input: Input = null
 ================================
  !! Expectation Failed: Fails
 ================================
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
#### Nested Collections Inner Count

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count <= 4", () => input.Count() <= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H1_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1860909202
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ _, _, _, _, _ ]
   - Trace: Original = [ [ 34, 19 ], [ 62, 47, 14, 22, 13 ], [ 64 ], [ 42, 5, 78, 63, 94 ], [ 4, 7, 91, 94 ] ]
 ==========================================
  !! Expectation Failed: List count <= 4
 ==========================================
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
 Location:                I_WhenAnEmptyCollectionIsTheCulprit.cs:32:1
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
 Location:                J1_TheCaseOfTheInvisibleCollection.cs:50:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
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
        from check in Checkr.Expect("Has 1", () => NewMethod(thing))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints",
        Fuzzr.Int().Many(2),
        Shrink.OnType<List<int>>(
            s => s.Classify(Try.Default<List<int>>(), Try.Values(thing.Ints)),
            s => s.Simplify(Reduce.Values([[], thing.Ints]))))
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from trace in Checkr.Trace("Original", () => thing.Ints)
        from check in Checkr.Expect("Has 1", () => NewMethod(thing))
        select Case.Closed))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J2_TheCaseOfTheInvisibleCollection.cs:50:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
   - Input: Ints     = [ 57, 70 ]
            Redux      [ ]
   - Trace: Original = [ 57, 70 ]
   - WARNING: Unused shrinkers found for 'List`1,List`1' at key 'Ints'.
 ================================
  !! Expectation Failed: Has 1
 ================================
 Passed Expectations
 - Has 1: 1x
 ------------------------------------------------------------
```
##### The Better Solution
Use the built-in `Shrink.CollectionFromState` shrinker, 
which is a convenience function that hides the plumbing required to define this type of shrinker.  

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
        Shrink.CollectionFromState(thing.Ints, [])) // <= Built-in Utility Shrinker
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
        select Case.Closed))
select Case.Closed;
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
#### Collection Is Null

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>(null!))
from expect in Checkr.Expect("Always fails.", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                P_CollectionIsNull.cs:32:1
 Original failing run:    1 execution
 Seed:                    849984174
 ------------------------------------------------------------
  Executed:
   - Input: List = null
   - WARNING: Null value for type 'IEnumerable`1' at input 'List'.
 ========================================
  !! Expectation Failed: Always fails.
 ========================================
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
from everything in Checkr.Input("Everything", TheLifeFuzzr,
    Shrink.OnType<TheUniverse.Life?>(s => s.Classify(Try.Fuzzr(TheLifeFuzzr))))
from irrelevant in Checkr.Input("Irrelevant", TheLifeFuzzr,
    Shrink.OnType<TheUniverse.Life?>(s => s.Classify(Try.Fuzzr(TheLifeFuzzr))))
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
#### Tuples

**The Checkr:**  
```csharp
from input in Checkr.Input("Tuple",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    from c in Fuzzr.Int()
    select (a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("Sum is < 42.", () => input.a + input.b < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J_Tuples.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    751254928
 ------------------------------------------------------------
  Executed:
   - Input: Tuple    = ( 52, 25, _ )
            Redux      ( 17, 25, _ )
   - Trace: Original = ( 52, 25, 23 )
 =======================================
  !! Expectation Failed: Sum is < 42.
 =======================================
```
#### Tuple Of Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Tuple",
    from a in Fuzzr.Int().Many(2)
    from b in Fuzzr.Int().Many(2)
    from c in Fuzzr.Int().Many(0)
    select (a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("First list does not contain 42.", () => !input.a.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                K_TupleOfLists.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1892439403
 ------------------------------------------------------------
  Executed:
   - Input: Tuple    = ( [ 42 ], _, _ )
   - Trace: Original = ( [ 42, 32 ], [ 17, 69 ], [ ] )
 ==========================================================
  !! Expectation Failed: First list does not contain 42.
 ==========================================================
```
#### Object Of Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Museum",
    from a in Fuzzr.Int().Many(2).ToList()
    from b in Fuzzr.Int().Many(2).ToList()
    from c in Fuzzr.Int().Many(0).ToList()
    select new Museum(a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("First list does not contain 42.", () => !input.A.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Q_ObjectOfLists.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 6 shrinks)
 Seed:                    1622405126
 ------------------------------------------------------------
  Executed:
   - Input: Museum   = { A: [ 42 ] }
   - Trace: Original = { A: [ 42, 6 ], B: [ 53, 25 ], C: [ ] }
 ==========================================================
  !! Expectation Failed: First list does not contain 42.
 ==========================================================
```
### Challenges

Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickCheckr,
but for performance reasons we do not run deliberation policies everytime for instance.
  
#### Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Bound5", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of all values is less than 5 * 256.",
    () => Sum(input) < (5 * 256))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<(List<short>, List<short>, List<short>, List<short>, List<short>)>(
            "Bound5", a => ElementCount(a)),
    DeliberationTarget = 2
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Bound5.cs:32:1
 Original failing run:    1 execution
 Seed:                    1866106578
 ------------------------------------------------------------
  Executed:
   - Input: Bound5   = ( _, [ -21169 ], _, [ -13193 ], _ )
   - Trace: Original = ( [ ], [ -8290, -21169 ], [ ], [ -13193 ], [ ] )
 ======================================================================
  !! Expectation Failed: The sum of all values is less than 5 * 256.
 ======================================================================
```
#### Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("LargeUnionList", TheFuzzr,
        Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "No more than four distinct integers.",
    () => input.SelectMany(a => a).Distinct().Count() <= 4)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("LargeUnionList", a => 0 - a.Count),
    DeliberationTarget = -5
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LargeUnionList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 25 shrinks)
 Seed:                    1284201255
 ------------------------------------------------------------
  Executed:
   - Input: LargeUnionList = [ [ _ ], [ 98 ], [ 33 ], [ 41 ], [ 73 ] ]
            Redux            [ [ _ ], [ 0 ], [ 1 ], [ 2 ], [ 3 ] ]
   - Trace: Original       = [ [ 37, 88, 82, 64, 26, 15, 91, 65 ], [ 76, 17, 18, 41, 82, 29, 89, 80, 24 ], [ 74, 39, 69, 94, 76, 44, 14, 66 ], [ 32, 98 ], [ 16, 25, 11, 59, 67, 74, 9 ], [ 63, 55, 4 ], [ ], [ 96 ], [ 16, 42, 86 ], [ 92, 57, 74, 11, 38, 67 ], [ 7, 21, 13, 49, 72, 34, 88, 10, 1, 92 ], [ 24, 61, 16 ], [ 96, 12, 20, 36, 67, 80, 80 ], [ 98 ], [ 33, 73 ], [ ], [ 41 ], [ 73 ] ]
 ===============================================================
  !! Expectation Failed: No more than four distinct integers.
 ===============================================================
```
#### Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("ReverseList", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from reverse in Checkr.Act("Reverse List",
    () => { var list = new List<int>(input); list.Reverse(); return list; })
from expect in Checkr.Expect(
    "Reversing a list of integers results in the same list",
    () => reverse.SequenceEqual(input))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReverseList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    767678629
 ------------------------------------------------------------
  Executed: Reverse List
   - Input: ReverseList = [ _, 92 ]
   - Trace: Original    = [ 68, 89, 98, 88, 92 ]
 ================================================================================
  !! Expectation Failed: Reversing a list of integers results in the same list
 ================================================================================
```
#### Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Fuzzr:**  
```csharp
from i in Configr.Primitive(Fuzzr.Int(0, 10))
from noZeroDiv in Configr<Expr>.Apply(
    a =>
    {
        // rewrite division by zero terms
        if (a is Term t)
            if (t.Op == Operation.Div && t.R is Num { Value: 0 })
                t.R = new Num { Value = 1 };
    })
from depth in Configr<Expr>.Depth(0, 10)
from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Num))
from terminator in Configr<Expr>.EndOn<Num>()
from expr in Fuzzr.One<Expr>()
select expr
```

**The Checkr:**  
```csharp
from input in Checkr.Input("Calculator", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Calculator.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1264796639
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = { Op: Div, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
   - Trace: Original   = { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
#### Calculator Adding A Reducer
And a DeliberationPolicy.  

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from input in Checkr.Input("Calculator", TheFuzzr,
    Shrink.When<int>(s => s
            .As<Num>(a => a.Value)
            .As<Term, Expr>(a => a.R)
            .Value(t => t.Op == Operation.Div),
        s => s.Simplify(Reduce.Towards(1))),
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<Expr>("Calculator", a => DepthOf(a)),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CalculatorAddingAReducer.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 12 shrinks)
 Seed:                    376086434
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = ( _ / ( 8 / _ ) )
            Redux        ( _ / ( 0 / _ ) )
   - Trace: Original   = { Op: Div, L: { Op: Add, L: { Value: 4 }, R: { Value: 6 } }, R: { Op: Div, L: { Value: 8 }, R: { Value: 9 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
#### Length List

A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Length List", TheFuzzr,
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The maximum value of the list is smaller than 900.",
    () => input.Max() < 900)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LengthList.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    1834085712
 ------------------------------------------------------------
  Executed:
   - Input: Length List = [ 962 ]
            Redux         [ 900 ]
   - Trace: Original    = [ 974, 962, 872, 681, 543, 118, 578 ]
 ============================================================================
  !! Expectation Failed: Maximum value of the list is at smaller than 900.
 ============================================================================
```
#### Difference
##### Test1

Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be zero.",
    () => input.A < 10 || input.A != input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test1.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1040200141
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 21, B: 21 }
            Redux      { A: 10, B: 10 }
   - Trace: Original = { A: 21, B: 21 }
 =======================================================
  !! Expectation Failed: difference must not be zero.
 =======================================================
```
##### Test2

Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(
        Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be small.",
    () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    737468544
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 53, B: 51 }
            Redux      { A: 10, B: 8 }
   - Trace: Original = { A: 53, B: 51 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
##### Test2 Better

This uses a custom reducer to shrink the input.  
It currently uses the full reducer algorithm.  
Working on turning this into a much easier way to define the reducer.  
But chaining reducers is still a work in progress.  
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(PairReducer.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be small.",
    () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
private class PairReducer
{
    public static Func<Reducer<Pair>> TowardsZero = () => new PairReducer().Reduce;
    private enum Phase { Both, A, B }
    private Phase phase = Phase.Both;
    public Reduction Reduce(ReductionResult result)
    {
        var current = result.ValueAs<Pair>();
        if (result.Passed)
        {
            if (phase == Phase.Both)
                phase = Phase.A;
            else if (phase == Phase.A)
                phase = Phase.B;
            if (phase == Phase.B)
                return new Reduction.Stop(current);
        }
        switch (phase)
        {
            case Phase.Both:
                if (current.A <= 0 || current.B == 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1, B = current.B - 1 });
            case Phase.A:
                if (current.A <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { B = current.B - 1 });
            case Phase.B:
                if (current.B <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1 });
        }
        return new Reduction.Stop(current);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2Better.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    737468544
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 53, B: 51 }
            Redux      { A: 10, B: 6 }
   - Trace: Original = { A: 53, B: 51 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
##### Test3

Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference is not exactly 1.",
    () => input.A < 10 || Math.Abs(input.A - input.B) != 1)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test3.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1238621164
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 49, B: 48 }
            Redux      { A: 10, B: 9 }
   - Trace: Original = { A: 49, B: 48 }
 =======================================================
  !! Expectation Failed: Difference is not exactly 1.
 =======================================================
```
This one sometimes produces 10, 11  
#### Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Heap", TheFuzzr)
from correct in Checkr.Act("Correct", () => Flatten(input).OrderBy(x => x).ToList())
from buggy in Checkr.Act("Buggy", () => Flatten(input).ToList())
from expect in Checkr.Expect("Heap sort is correct", () => correct.SequenceEqual(buggy))
from trace in Checkr.Trace("Original", () => input)
select Case.Closed;
```

**The Fuzzr:**  
```csharp
from depth in Configr<Heap>.Depth(3, 20)
from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
from terminator in Configr<Heap>.EndOn<Empty>()
from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
from value in Configr<Node>.Property(a => a.Value,
    from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
from heap in Fuzzr.One<Heap>()
select heap
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
       (caseFile.InputCount == 0 ? 100 : 0) +
       caseFile.InputsNamed<Heap>("Heap", a => Flatten(a).Count()),
    ReportMode = config.ReportMode & ~ReportMode.Warning
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                BinHeap.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    195567586
 ------------------------------------------------------------
  Executed: Buggy, Correct
   - Input: Heap     = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
   - Trace: Original = { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ===============================================
  !! Expectation Failed: Heap sort is correct
 ===============================================
```
#### Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Checkr:**  
```csharp
from length in Checkr.Input("Length", Fuzzr.Int(1, 100))
from list in Checkr.Input("List", () =>
    from enumerable in Fuzzr.Int(0, length - 1).Many(length)
    select enumerable.ToList())
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("No two different indexes point to each other.",
    () => list.All(element => element < list.Count),
    () => list.All(element =>
    {
        int index = list[element];
        if (index != element && list[index] == element)
            return false;
        return true;
    }))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<List<int>>("List", a => a.Count),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Coupling.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    124252565
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 1, 0 ]
   - Trace: Original = [ 2, 1, 0, 1 ]
 ========================================================================
  !! Expectation Failed: No two different indexes point to each other.
 ========================================================================
```
#### Deletion

This tests the property "if we remove an element from a list, the element is
no longer in the list". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Deletion", Fuzzr.Int(1, 10).Many(3, 20))
from toRemove in Checkr.Input("Element To Remove", () => Fuzzr.OneOf(input))
from removed in Checkr.Act("Remove Element", () =>
{
    var list = input.ToList();
    list.Remove(toRemove);
    return list;
})
from expect in Checkr.Expect("Element is no longer in the list", () => !removed.Contains(toRemove))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Deletion.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 16 shrinks)
 Seed:                    613616447
 ------------------------------------------------------------
  Executed: Remove Element
   - Input: Deletion          = [ 3, 3 ]
   - Input: Element To Remove = 3
 ===========================================================
  !! Expectation Failed: Element is no longer in the list
 ===========================================================
```
#### Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Distinct", Fuzzr.Int(1, 10).Many(3, 20),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen(
    "Contains at least three distinct elements.",
    () => input.Count() >= 3,
    () => input.ToHashSet().Count >= 3)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Distinct.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    767903368
 ------------------------------------------------------------
  Executed:
   - Input: Distinct = [ _, 8, 8 ]
   - Trace: Original = [ 5, 8, 8 ]
 =====================================================================
  !! Expectation Failed: Contains at least three distinct elements.
 =====================================================================
```
#### Nested Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 10)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("Nested List", a => a.Count),
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedLists.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 30 shrinks)
 Seed:                    1716272349
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _, _, _, _, _, _, _, _, _, _ ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
