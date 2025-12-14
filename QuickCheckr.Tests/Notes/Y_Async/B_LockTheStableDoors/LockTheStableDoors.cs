using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
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
        var article = TheJournalist.Exposes(() =>
            TheCheckr.Run(1511313504, 20.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport<LockTheStableDoors>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Assigned Coaches Must Be Suitable", article.FailedExpectation());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(5, article.Total().Traces());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(12, article.ShrinkCount);
        Assert.Equal(4, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Coach", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Coach Name", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("Coach-1", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(5, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Course", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Course Name", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("Course-2", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal(6, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Assign Coach to Course", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Coach Name", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("Coach-1", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal("Course Name", article.Execution(3).Trace(2).Read().Label);
        Assert.Equal("Course-2", article.Execution(3).Trace(2).Read().Value);
        Assert.Equal(11, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Update Course Skills", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Course Skills", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("[ _ ]", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Course Name", article.Execution(4).Trace(1).Read().Label);
        Assert.Equal("Course-2", article.Execution(4).Trace(1).Read().Value);
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
    private static readonly CheckrOf<Case> CreateCoach =
        from service in Trackr.Stashed<InMemoryCoachCourseService>()
        from name in Checkr.Input("Coach Name",
            from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
        from trace in Checkr.Trace("Coach Name", () => name)
        from coach in Checkr.Act("Create Coach", service.CreateCoachAsync(name))
        from toPool in Trackr.Pool("Store Coach", new CoachBy(name))
        select Case.Closed;

    [CodeExample]
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

    [CodeExample]
    private static readonly CheckrOf<Case> CreateCourse =
        from service in Trackr.Stashed<InMemoryCoachCourseService>()
        from name in Checkr.Input("Course Name",
            from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
        from trace in Checkr.Trace("Course Name", () => name)
        from course in Checkr.Act("Create Course", service.CreateCourseAsync(name))
        from toPool in Trackr.Pool("Store Course", new CourseBy(name))
        select Case.Closed;

    [CodeExample]
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

    [CodeExample]
    private static readonly CheckrOf<Case> AssignCoachToCourse =
        from service in Trackr.Stashed<InMemoryCoachCourseService>()
        from coach in Trackr.OneOfPool<CoachBy>("Coach to assign")
        from course in Trackr.OneOfPool<CourseBy>("Course to assign")
        from coaches in Checkr.Capture(service.GetCoachesAsync())
        from courses in Checkr.Capture(service.GetCoursesAsync())
        from dbCoach in Checkr.Capture(() => coaches.Single(a => a.Name == coach.Name))
        from dbCourse in Checkr.Capture(() => courses.Single(a => a.Name == course.Name))
        from valid in Checkr.When(() => coach != null && course != null && dbCoach.IsSuitableFor(dbCourse),
            from traceCoach in Checkr.Trace("Coach Name", () => coach.Name)
            from traceCourse in Checkr.Trace("Course Name", () => course.Name)
            from assign in Checkr.Act("Assign Coach to Course", service.AssignCoachToCourseAsync(coach.Name, course.Name))
            select Case.Closed)
        select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from service in Trackr.Stashed<InMemoryCoachCourseService>()
        from coaches in Checkr.Capture(service.GetCoachesAsync())
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
        from coaches in Trackr.PoolFor<CoachBy>()
        from courses in Trackr.PoolFor<CourseBy>()
        from dbCourses in Checkr.Capture(service.GetCoursesAsync())
        from operations in Checkr.OneOfIf(
            (() => coaches.Count() < 10, CreateCoach),
            (() => coaches.Any(), UpdateCoachSkills),
            (() => courses.Count() < 5, CreateCourse),
            (() => courses.Any(), UpdateCourseSkills),
            (() => coaches.Any() && dbCourses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;
}