using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Y_Async.B_LockTheStableDoors;

[DocFile]
public class LockTheStableDoors : BaseTest
{
    [Fact]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(LockTheStableDoors), nameof(TheCheckr))]
    [DocContent("**Create Coach Checkr:**")]
    [DocExample(typeof(LockTheStableDoors), nameof(CreateCoach))]
    [DocContent("**Update Coach Skills Checkr:**")]
    [DocExample(typeof(LockTheStableDoors), nameof(UpdateCoachSkills))]
    [DocContent("**Create Course Checkr:**")]
    [DocExample(typeof(LockTheStableDoors), nameof(CreateCourse))]
    [DocContent("**Update Course Skills Checkr:**")]
    [DocExample(typeof(LockTheStableDoors), nameof(UpdateCourseSkills))]
    [DocContent("**Assigned Coaches Must Be Suitable Checkr:**")]
    [DocExample(typeof(LockTheStableDoors), nameof(AssignedCoachesMustBeSuitable))]
    [DocContent("**The Report:**")]
    [DocCodeFile("LockTheStableDoors.txt", "text")]
    public void RunTest()
    {
        var article = Journalist.Publish(TheCheckr, a => a
                .Run(1511313504, 20.ExecutionsPerRun()));

        WriteAsserts(article, false);
        WriteReport<LockTheStableDoors>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Assigned Coaches Must Be Suitable", article.FailureDescription());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(3, article.Total().PoolTraces());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(5, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Coach", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Store Coach", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("CoachBy-2", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(6, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Course", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Store Course", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("CourseBy-2", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal(8, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Assign Coach to Course", article.Execution(3).Action(1).Read().Label);
        Assert.Equal(13, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Update Course Skills", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Course Skills", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("[ _ ]", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Course to update", article.Execution(4).PoolTrace(1).Read().Label);
        Assert.Equal("CourseBy-2", article.Execution(4).PoolTrace(1).Read().Value);
        Assert.Equal("Assigned Coaches Must Be Suitable", article.PassedExpectation(1).Read().Label);
        Assert.Equal(5, article.PassedExpectation(1).Read().TimesPassed);
    }

    private static readonly string[] SkillPool =
    [
        "TDD", "Refactoring", "C#", "ASP.NET", "EF Core", "SQL",
        "DomainDrivenDesign", "UnitTesting", "Git", "CI/CD",
        "JavaScript", "React", "Elm", "Architecture"
    ];


    [CodeExample]
    private static CheckrOf<Case> CreateCoach(InMemoryCoachCourseService service) =>
        Poolr.Add("Store Coach",
            from name in Checkr.Input("Coach Name",
                from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
            from coach in Checkr.Act("Create Coach", () => service.CreateCoachAsync(name))
            select new CoachBy(name));

    [CodeExample]
    private static CheckrOf<Case> UpdateCoachSkills(InMemoryCoachCourseService service) =>
        from coach in Poolr.OneOf<CoachBy>("Coach to update")
        from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => service.UpdateCoachSkillsAsync(coach.Name, skills))
        select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> CreateCourse(InMemoryCoachCourseService service) =>
        Poolr.Add("Store Course",
            from name in Checkr.Input("Course Name",
                from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
            from course in Checkr.Act("Create Course", () => service.CreateCourseAsync(name))
            select new CourseBy(name));

    [CodeExample]
    private static CheckrOf<Case> UpdateCourseSkills(InMemoryCoachCourseService service) =>
        from course in Poolr.OneOf<CourseBy>("Course to update")
        from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.Act("Update Course Skills",
            () => service.UpdateCourseSkillsAsync(course.Name, skills))
        select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> AssignCoachToCourse(InMemoryCoachCourseService service) =>
        from pool in Trackr.OneOfPool<CoachBy, CourseBy>("Coach to assign", (coach, course) =>
            from coaches in Checkr.Capture(service.GetCoachesAsync)
            from courses in Checkr.Capture(service.GetCoursesAsync)
            from dbCoach in Checkr.Capture(() => coaches.Single(a => a.Name == coach.Name))
            from dbCourse in Checkr.Capture(() => courses.Single(a => a.Name == course.Name))
            from valid in Checkr.When(() => dbCoach.IsSuitableFor(dbCourse), // we can do better than this, i'm sure ;-)
                from assign in Checkr.Act("Assign Coach to Course", () => service.AssignCoachToCourseAsync(coach.Name, course.Name))
                select Case.Closed)
            select Case.Closed)
        select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> AssignedCoachesMustBeSuitable(InMemoryCoachCourseService service) =>
        from coaches in Checkr.Capture(service.GetCoachesAsync)
        from assignedCheck in Checkr.ExpectWhen("Assigned Coaches Must Be Suitable",
            () => coaches.Any(coach => coach.AssignedCourses.Any()),
            () => coaches.All(
                coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course))))
        select Case.Closed;

    public record CoachBy(string Name);
    public record CourseBy(string Name);

    [CodeExample]
    private static readonly CheckrOf<Case> TheCheckr =
        from service in Trackr.Stashed(() => new InMemoryCoachCourseService())
        from dbCourses in Checkr.Capture(service.GetCoursesAsync)
        from operations in Checkr.OneOfWhen(
            Poolr.When<CoachBy>(a => a.Count < 10, CreateCoach(service)),
            Poolr.Has<CoachBy>(UpdateCoachSkills(service)),
            Poolr.When<CourseBy>(a => a.Count < 5, CreateCourse(service)),
            Poolr.Has<CourseBy>(UpdateCourseSkills(service)),
            Poolr.Has<CoachBy>()
                .AndWhen<CourseBy>(a => a.Any(course =>
                    dbCourses.Single(b => b.Name == course.Name).AssignedCoach == null))
                .Then(AssignCoachToCourse(service)))
        from invariant in AssignedCoachesMustBeSuitable(service)
        select Case.Closed;
}