using QuickCheckr;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;

[DocFile]
[DocContent(
@"For this example we are going the use a simplified version of the [`HorsesForCourses`](https://github.com/kilfour/HorsesForCourses/blob/main/README.md) domain.  

The bug we're hunting was actually found in the wild (although I simplified the code, in order to keep it short)
and is demonstrative for a whole category: *Invariants drifting over time*.
")]
[DocContent("**The Coach:**")]
[DocExample(typeof(Coach))]
[DocContent("**The Course:**")]
[DocExample(typeof(Course))]
public class BoltingHorses : BaseTest
{
    [Fact]
    [DocContent(
@"**The Checkr:**  
This one is the *entry point* for the test.  
It creates two pools, one for coaches, and one for courses, which we are going to use to track the state of the system.  
It then creates a series of actions, randomly choosing one, using `Checkr.OneOfIf`, out of those in the list for which it's predicate is fulfilled.
Finally after each action we check the `AssignedCoachesMustBeSuitable` Invariant.")]
    [DocExample(typeof(BoltingHorses), nameof(TheCheckr))]
    [DocContent(
@"**Create Coach Checkr:**  
In this one we create a coach with a random name, verify that the property `Name` is assigned and then add it to the coaches pool.")]
    [DocExample(typeof(BoltingHorses), nameof(CreateCoach))]
    [DocContent(
@"**Update Coach Skills Checkr:**  
Now we retrieve a random coach from the pool and update its skills, verifying that the property `Skills` is set correctly.")]
    [DocExample(typeof(BoltingHorses), nameof(UpdateCoachSkills))]
    [DocContent(
@"**Create Course Checkr:**  
Same pattern as the *Create Coach Checkr*.")]
    [DocExample(typeof(BoltingHorses), nameof(CreateCourse))]
    [DocContent(
@"**Update Course Skills Checkr:**  
And again, same pattern as the *Update Coach Skills Checkr*.")]
    [DocExample(typeof(BoltingHorses), nameof(UpdateCourseSkills))]
    [DocContent(
@"**Assigned Coaches Must Be Suitable Checkr:**  
And finally here's the system invariant. Any coach assigned a course must have the required skills to teach that course.")]
    [DocExample(typeof(BoltingHorses), nameof(AssignedCoachesMustBeSuitable))]
    [DocContent("**The Report:**")]
    [DocCodeFile("BoltingHorses.txt", "text")]
    [DocContent(
@"The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment."
    )]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() => TheCheckr.Run(13918324, 20.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport<BoltingHorses>(article);
        Assert.Equal("Assigned Coaches Must Be Suitable", article.FailedSpec());
        Assert.Equal("Assigned Coaches Must Be Suitable", article.FailedSpec());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(5, article.Total().Traces());
        Assert.Equal(21, article.ShrinkCount);
        Assert.Equal(8, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Coach", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Coach Name", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("Coach-5", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(10, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Course", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Course Name", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("Course-3", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal(13, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Assign Coach to Course", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Coach Name", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("Coach-5", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal("Course Name", article.Execution(3).Trace(2).Read().Label);
        Assert.Equal("Course-3", article.Execution(3).Trace(2).Read().Value);
        Assert.Equal(20, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Update Course Skills", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Course Skills", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("[ _ ]", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Course Name", article.Execution(4).Trace(1).Read().Label);
        Assert.Equal("Course-3", article.Execution(4).Trace(1).Read().Value);
    }
    private static readonly string[] SkillPool =
    [
        "TDD", "Refactoring", "C#", "ASP.NET", "EF Core", "SQL",
        "DomainDrivenDesign", "UnitTesting", "Git", "CI/CD",
        "JavaScript", "React", "Elm", "Architecture"
    ];


    [CodeExample]
    private static readonly CheckrOf<Case> CreateCoach =
        from name in Checkr.Input("Coach Name",
            from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
        from trace in Checkr.Trace("Coach Name", () => name)
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Spec("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.Pool("Store Coach", coach)
        select Case.Closed;

    [CodeExample]
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

    [CodeExample]
    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name",
            from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
        from trace in Checkr.Trace("Course Name", () => name)
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Spec("Course Name Set", () => course.Name == name)
        from toPool in Trackr.Pool("Store Course", course)
        select Case.Closed;

    [CodeExample]
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

    [CodeExample]
    private static readonly CheckrOf<Case> AssignCoachToCourse =
        from coach in Trackr.OneOfPool<Coach>("Coach to assign")
        from course in Trackr.OneOfPool<Course>("Course to assign")
        from valid in Checkr.When(() => coach != null && course != null && coach.IsSuitableFor(course),
            from traceCoach in Checkr.Trace("Coach Name", () => coach.Name)
            from traceCourse in Checkr.Trace("Course Name", () => course.Name)
            from assign in Checkr.Act("Assign Coach to Course", () => course.AssignCoach(coach))
            from isAssigned in Checkr.Spec("Coach is assigned", () => course.AssignedCoach == coach)
            select Case.Closed)
        select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from coaches in Trackr.Pool<Coach>()
        from assignedCheck in Checkr.SpecIf("Assigned Coaches Must Be Suitable",
            () => coaches.Any(coach => coach.AssignedCourses.Any()),
            () => coaches.All(
                coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course))))
        select Case.Closed;

    [CodeExample]
    public static readonly CheckrOf<Case> TheCheckr =
        from coaches in Trackr.PoolFor<Coach>()
        from courses in Trackr.PoolFor<Course>()
        from operations in Checkr.OneOfIf(
            (() => coaches.Count() < 10, CreateCoach),
            (() => coaches.Any(), UpdateCoachSkills),
            (() => courses.Count() < 5, CreateCourse),
            (() => courses.Any(), UpdateCourseSkills),
            (() => coaches.Any() && courses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;
}