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
