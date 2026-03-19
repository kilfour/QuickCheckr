using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.E_GatheringEvidence;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;

[DocFile]
[DocLink(typeof(Guide))]
[DocLink(typeof(InPoolsWeTrust))]
[DocLink(typeof(GatheringEvidence))]
[DocContent("[Guide][Guide] | [Previous][InPoolsWeTrust]  \n")]
[DocContent(
@"Up to this point we have seen a few examples of how to use QuickCheckr to test a system, but the bugs have been relatively trivial.
For this one, we're going to look at a real bug found in the wild, which is demonstrative for a whole category: *Invariants drifting over time*.

> This is the kind of bug QuickCheckr is built to find.

*So, here we go:*  
We are going to  use a simplified version of the [`HorsesForCourses`](https://github.com/kilfour/HorsesForCourses/blob/main/README.md) domain.  

The invariant we are checking is : 
- *Any coach assigned a course must have the required skills to teach that course*.

And here's the domain model:
")]
[DocContent("**The Coach:**")]
[DocExample(typeof(Coach))]
[DocContent("**The Course:**")]
[DocExample(typeof(Course))]
public class BoltingHorses : BaseTest
{
    [Fact]
    [DocContent("Our Checkr is composed out of several parts:")]
    [DocContent(
@"

**The Composed Checkr:**  
This one is the *entry point* for the test.  
It creates two pools, one for coaches, and one for courses, which we are going to use to track the state of the system.  
It then creates a series of actions, randomly choosing one, using `Checkr.OneOfWhen`,
out of those in the list for which it's predicate is fulfilled.  
Finally after each action we check the `AssignedCoachesMustBeSuitable` Invariant.")]
    [DocExample(typeof(BoltingHorses), nameof(TheCheckr))]
    [DocContent(
@"**Create Coach Checkr:**  
In this one we create a coach with a *semi* random name, verify that the property `Name` is assigned and then add it to the coaches pool.")]
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
@"**Assign Coach to Course Checkr:**")]
    [DocExample(typeof(BoltingHorses), nameof(AssignCoachToCourse))]
    [DocContent(
@"**Update Course Skills Checkr:**  
And again, same pattern as the *Update Coach Skills Checkr*.")]
    [DocExample(typeof(BoltingHorses), nameof(UpdateCourseSkills))]
    [DocContent(
@"**Assigned Coaches Must Be Suitable Checkr:**  
And finally here's the system invariant.  
*Any coach assigned a course must have the required skills to teach that course*.")]
    [DocExample(typeof(BoltingHorses), nameof(AssignedCoachesMustBeSuitable))]
    [DocContent("**The Report:**")]
    [DocCodeFile("BoltingHorses.txt", "text")]
    [DocContent(
@"The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment.

**Note:** The astute reader will have noticed there exists more than one way in which the invariant can be violated.  
Let me redirect said readers to [Gathering Evidence][GatheringEvidence]."
    )]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() => TheCheckr.Run(1473455315, 20.ExecutionsPerRun(),
            a => a with
            {
                ReportMode = a.ReportMode & ~ReportMode.Warning
            }));
        WriteAsserts(article, false);
        WriteReport<BoltingHorses>(article, false);
        Verify(article);
    }

    private static void Verify(Tools.ThePress.Printing.Article article)
    {
        Assert.Equal("Assigned Coach Must Be Suitable", article.FailureDescription());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(4, article.Total().PoolTraces());
        Assert.Equal(6, article.Total().PassedExpectations());
        Assert.Equal(14, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Course", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Store Course", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Course-1", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(4, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Coach", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Store Coach", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("Coach-1", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal(10, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Assign Coach to Course", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("(Coach, Course) to Assign", article.Execution(3).PoolTrace(1).Read().Label);
        Assert.Equal("( Coach-1, Course-1 )", article.Execution(3).PoolTrace(1).Read().Value);
        Assert.Equal(13, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Update Course Skills", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Course Skills", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("[ _ ]", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Course to Update Skills", article.Execution(4).PoolTrace(1).Read().Label);
        Assert.Equal("Course-1", article.Execution(4).PoolTrace(1).Read().Value);
        Assert.Equal("Course Name Set", article.PassedExpectation(1).Read().Label);
        Assert.Equal(4, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("Coach Name is set", article.PassedExpectation(2).Read().Label);
        Assert.Equal(4, article.PassedExpectation(2).Read().TimesPassed);
        Assert.Equal("Coach Skills Set", article.PassedExpectation(3).Read().Label);
        Assert.Equal(2, article.PassedExpectation(3).Read().TimesPassed);
        Assert.Equal("Coach is assigned", article.PassedExpectation(4).Read().Label);
        Assert.Equal(2, article.PassedExpectation(4).Read().TimesPassed);
        Assert.Equal("Assigned Coach Must Be Suitable", article.PassedExpectation(5).Read().Label);
        Assert.Equal(3, article.PassedExpectation(5).Read().TimesPassed);
        Assert.Equal("Course Required Skills Set", article.PassedExpectation(6).Read().Label);
        Assert.Equal(1, article.PassedExpectation(6).Read().TimesPassed);
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
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.ToPool("Store Coach", coach) // Store this coach in the pool.
        select Case.Closed;

    [CodeExample]
    private static readonly Func<PoolElement<Coach>, CheckrOf<Case>> UpdateCoachSkills =
        coach =>
            from skills in Checkr.Input("Coach Skills",
                Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(1, 3))
            from updateSkills in Checkr.Act("Update Coach Skills",
                () => coach.Value.UpdateSkills(skills)) // Update the Coach.
            from checkSkills in Checkr.Expect("Coach Skills Set",
                () => coach.Value.Skills.Count
                   == skills.Count() && coach.Value.Skills.All(skills.Contains))
            select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name",
            from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course) // Store this course in the pool.
        select Case.Closed;

    [CodeExample]
    private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
        course =>
            from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(1, 3))
            from updateSkills in Checkr.Act("Update Course Skills",
                () => course.Value.UpdateRequiredSkills(skills)) // Update the Course.
            from checkSkills in Checkr.Expect("Course Required Skills Set",
                () => course.Value.RequiredSkills.Count
                   == skills.Count() && course.Value.RequiredSkills.All(skills.Contains))
            select Case.Closed;

    [CodeExample]
    private static readonly Func<PoolElement<Coach>, PoolElement<Course>, CheckrOf<Case>> AssignCoachToCourse =
        (coach, course) =>
            from assign in Checkr.Act("Assign Coach to Course", () => course.Value.AssignCoach(coach.Value))
            from isAssigned in Checkr.Expect("Coach is assigned", () => course.Value.AssignedCoach == coach.Value)
            select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from assignedCheck in Trackr.PoolExpectEachWhen<Coach>("Assigned Coach Must Be Suitable",
            coach => coach.AssignedCourses.Any(), // Check that all assigned coaches are suitable.
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
        select Case.Closed;

    [CodeExample]
    public static readonly CheckrOf<Case> TheCheckr =
        from coaches in Trackr.PoolFor<Coach>()  // Create a pool of coaches.
        from courses in Trackr.PoolFor<Course>() // Create a pool of courses.
        from operations in Checkr.OneOfWhen(
            // Create up to 10 coaches.
            Trackr.PoolWhen<Coach>(a => a.Count < 10, CreateCoach),
            // Maybe update existing coaches. Pick a random coach from the pool.
            Trackr.PoolWhen("Coach to Update Skills", a => a.Count != 0, UpdateCoachSkills),
            // Create up to 5 courses.
            Trackr.PoolWhen<Course>(a => a.Count < 5, CreateCourse),
            // Maybe update existing courses. Pick a random course from the pool.
            Trackr.PoolWhen("Course to Update Skills", a => a.Count != 0, UpdateCourseSkills),
            // Maybe assign a coach, if there exists a combination for which the predicate holds.
            Trackr.PoolWhen("(Coach, Course) to Assign",
                (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
            // Ensure the system invariant is met at all times.
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;
}