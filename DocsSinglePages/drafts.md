# Registered But Unused
Because custom shrinkers can be defined for *nested* types, mistakes are possible.
Hence this warning.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Try.Values(["unused"]))
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
## All Inputs Shrunk

**The Checkr:**  
```csharp
from input in Checkr.Input("An Input", Fuzzr.Int())
from expect in Checkr.Expect("Always fails", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                AllInputsShrunk.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1098916919
 ------------------------------------------------------------
  Executed:
   - WARNING: All inputs were considered irrelevant.
 =======================================
  !! Expectation Failed: Always fails
 =======================================
```
  
  
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
#### A Simple Scenario
##### Introduction
  
##### Spike

**The Model:**  
```csharp
public class Appointment
{
    public Appointment(string title)
    {
        EditTitle(title);
    }
    public string Title { get; private set; } = string.Empty;
    public void EditTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentNullException(nameof(title), "Title cannot be null or whitespace");
        Title = title;
    }
}
```

**The Checkr:**  
```csharp
from _i1 in Checkr.Input("Input, Create Appointment, Empty String", EvilString)
from _a1 in Checkr.ActCarefully("Act, Create Appointment, Empty String", () => new Appointment(_i1))
from _e1 in Checkr.Expect("Expect, Create Appointment, Empty String", () => _a1.Value.Title == _i1)
    //from _e1 in Checkr.Expect("Create Appointment, Empty String", () => _a1.ThrewExactly<ArgumentNullException>())
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Spike.cs:55:1
 Original failing run:    2 executions
 Seed:                    720678568
 ------------------------------------------------------------
  Executed: Act, Create Appointment, Empty String
   - Input, Create Appointment, Empty String = "udplclmhev"
 ------------------------------------------------------------
  Executed: Act, Create Appointment, Empty String
   - Input, Create Appointment, Empty String = ""
 ===========================================================================
  !! DelayedResultNullValueAccessed: Exception of type 'QuickCheckr.UnderTheHood.DelayedResultNullValueAccessed' was thrown.
 ===========================================================================
 Passed Expectations
 - Expect, Create Appointment, Empty String: 1x
 ------------------------------------------------------------
```
## Checkrs Court
The basic building block of QuickCheckr is a `CheckrOf<T>` (also, informally, called a *Checkr* throughout these docs). 

Multiple Checkrs can be combined into a composite Checkr using `LINQ` chains, as many of the examples show.  

The *atomic* Checkrs are created through a couple of static Factory classes :

- [The Checkr Class][TheCheckrClass]
  - Provides methods to create the *basic* Checkrs.
  - All of the Checkrs created by this class are *execution-scoped*.
- [The Tracker Class][TheTrackerClass]
  - Provides methods to create the Checkrs that deal with state-mutation, and/or Checkrs that are *run-scoped*.  
- [The Tracker Class][TheShowrClass]
  - Allows for customization of the QuickChecker value printer.  
  

[TheCheckrClass]: #the-checkr-class

[TheTrackerClass]: #the-tracker-class

[A_TheShowrClass]: #the-showr-class
### The Checkr Class
#### Input
`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.  

**Usage**  
```csharp
Checkr.Input("AnInt", Fuzzr.Int());
```
###### Exception Handling
The Checkr below will throw a QuickFuzzr `OneOfEmptyOptionsException`:  
```csharp
    from input in Checkr.Input("AnInt", Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
Whenever an input throws, the exception is wrapped into a `InputFuzzrThrewException`.  
```csharp
Input 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state.
And as 'Input' is eagerly evaluated, this can cause problems.
In the latter case please try using the 'lazy' version. 
Example: 'Checkr.Input(() => Fuzzr.Int()).
```
As you can gather from the exception message, this is done in order to inform the user of a rare, but real,
issue when a Fuzzr depends on external state.  
The solution is also supplied in said message:   
```csharp
Checkr.Input("AnInt", () => Fuzzr.Int());
```
If the *lazy* version throws:  
```csharp
    from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
The Exception is **not** wrapped.  
```csharp
Fuzzr.OneOf<Int32> cannot select from an empty sequence.
Possible solutions:
- Provide at least one option (ensure the sequence is non-empty).
- Use a fallback: Fuzzr.OneOf(values).WithDefault()
- Guard upstream: values.Any() ? Fuzzr.OneOf(values) : Fuzzr.Constant(default!).
```
### The Tracker Class
## Conditional Execution
### Checkr Act Carefully When

**The Checkr:**  
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
from thing in Checkr.ActCarefullyWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
from notNull in Checkr.Expect("Not Null", () => thing.HasValue)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CheckrActCarefullyWhen.cs:55:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    625676917
 ------------------------------------------------------------
  Executed: Act
   - Input: Counter = 3
 ===================================
  !! Expectation Failed: Not Null
 ===================================
 Passed Expectations
 - Not Null: 1x
 ------------------------------------------------------------
```
### Checkr Act When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from never in Checkr.ActWhen("Never", () => false, () => { })
    from neverAgain in Checkr.ActWhen("Never Again", () => false, () => true)
    from thing in Checkr.ActWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from never in Checkr.ActWhen("Never", () => false, () => Task.CompletedTask)
    from neverAgain in Checkr.ActWhen("Never Again", () => false, () => Task.FromResult(true))
    from thing in Checkr.ActWhen("Act", () => counter != 2, () => Task.FromResult(counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
### Checkr When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.When(() => counter != 2, Checkr.Act("Act", () => counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
## When Things Go Wrong
### Everything Means Less Than Null
#### Trackr To Pool
```csharp
    from one in Trackr.ToPool("answer", 42)
    select Case.Closed;
```
  Throws:  
```text
A storage attempt was made for a 'Pool' of 'System.Int32' using key 'answer', but no such pool was found.
Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
```
### It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
#### Checkr Act
```csharp
    from one in Checkr.Act("reused", () => { })
    from two in Checkr.Act("reused", () => { })
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
#### Checkr Act Carefully
```csharp
    from one in Checkr.ActCarefully("reused", () => { })
    from two in Checkr.ActCarefully("reused", () => { })
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
#### Checkr Expect
```csharp
    from one in Checkr.Expect("reused", () => true)
    from two in Checkr.Expect("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Checkr Input
```csharp
    from one in Checkr.Input("reused", Fuzzr.Int())
    from two in Checkr.Input("reused", Fuzzr.Int())
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Input'
```
#### Checkr One Of
```csharp
    from one in Checkr.OneOf(Checkr.Act("One", () => { }))
    from two in Checkr.OneOf(Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
    from one in Checkr.OneOf("reused", Checkr.Act("One", () => { }))
    from two in Checkr.OneOf("reused", Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
#### Checkr One Of When
```csharp
    from one in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("One", () => { })))
    from two in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("Two", () => { })))
    select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOfWhen|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
    from one in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("One", () => { })))
    from two in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("Two", () => { })))
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
#### Checkr Trace
```csharp
    from one in Checkr.Trace("reused", () => "")
    from two in Checkr.Trace("reused", () => "")
    from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
#### Checkr Trace When
```csharp
    from one in Checkr.TraceWhen("reused", () => true, () => "")
    from two in Checkr.TraceWhen("reused", () => true, () => "")
    from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
#### Trackr Expect
```csharp
    from one in Trackr.Expect("reused", () => true)
    from two in Trackr.Expect("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Trackr Pool For
```csharp
    from one in Trackr.PoolFor<int>()
    from two in Trackr.PoolFor<int>()
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.PoolFor'
```
#### Trackr Stashed
```csharp
    from one in Trackr.Stashed(() => 42)
    from two in Trackr.Stashed(() => 666)
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
```csharp
    from one in Trackr.Stashed("reused", () => 42)
    from two in Trackr.Stashed("reused", () => 666)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
#### Trackr Stop When Proven
```csharp
    from one in Trackr.StopWhenProven("reused", () => false)
    from two in Trackr.StopWhenProven("reused", () => false)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Trackr Verify
```csharp
    from one in Trackr.Verify("reused", () => true)
    from two in Trackr.Verify("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
## Issues
### The Unshrinkables
#### Unknown Primitives
```csharp
    from unkown in Checkr.Input("Half", Fuzzr.Half())
    from fail in Checkr.Expect("Fails", () => false)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\Z_Issues\A_TheUnshrinkables\B_UnknownPrimitives\UnknownPrimitives.cs:19:1
 Original failing run:    1 execution
 Seed:                    746257211
 ------------------------------------------------------------
  Executed:
   - Input: Half = 65,25
   - WARNING: Can't shrink type 'Half' at input 'Half'.
 ================================
  !! Expectation Failed: Fails
 ================================
```
### System Types
```csharp
public class Thing
{
    public int Irrelevant { get; set; }
    public Type Type { get; set; } = typeof(int);
    public Type OtherType { get; set; } = typeof(int);
}
```
```csharp
    from thing in Checkr.Input("A thing with a type", Fuzzr.Constant(new Thing()))
    from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1487819190
 ------------------------------------------------------------
  Executed:
   - Input: A thing with a type = { Type: Int32, OtherType: Int32 }
   - WARNING: Object of type 'Type' has no shrinkable properties at input 'A thing with a type.Type'.
   - WARNING: Object of type 'Type' has no shrinkable properties at input 'A thing with a type.OtherType'.
 ================================
  !! Expectation Failed: Fails
 ================================
```
##### Workaround
```csharp
    from thing in Checkr.Input("A thing with a type",
        Fuzzr.Constant(new Thing()),
        Try.Values([typeof(string), typeof(double)]))
    from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTestSolution
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    350297528
 ------------------------------------------------------------
  Executed:
   - Input: A thing with a type = { Type: Int32 }
 ================================
  !! Expectation Failed: Fails
 ================================
```
