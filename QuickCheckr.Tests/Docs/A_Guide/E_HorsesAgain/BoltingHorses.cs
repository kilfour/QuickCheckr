using QuickCheckr;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;

[DocFile]
[DocContent(
@"Up to this point we have seen a few examples of how to use QuickCheckr to test a system, but the bugs have been relatively trivial.
For this one, we're going to look at a real bug found in the wild, which is demonstrative for a whole category: *Invariants drifting over time*.

The only concept we haven't covered yet, and that we need here, is:

**Pools**  
These let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  
In this example, we'll keep all created coaches and courses in pools,
and each operation will pick from those pools to mutate the system over time.

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, pool operations (When, These, ForThese, ToPool) encapsulate ""is this state valid?"" so shrinking can replay safely.

*Let's go:*  
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
And finally here's the system invariant.  
*Any coach assigned a course must have the required skills to teach that course*.")]
    [DocExample(typeof(BoltingHorses), nameof(AssignedCoachesMustBeSuitable))]
    [DocContent("**The Report:**")]
    [DocCodeFile("BoltingHorses.txt", "text")]
    [DocContent(
@"The shrunk case shows exactly how the invariant drifted:  
A coach was assigned while suitable, then a course's required skills changed, invalidating that assignment."
    )]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() => TheCheckr.Run(1073869613, 20.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<BoltingHorses>(article, false);
        ExplainMe<BoltingHorses>(false);
        Verify(article);
    }

    private static void Verify(Tools.ThePress.Printing.Article article)
    {
        Assert.Equal("Assigned Coach Must Be Suitable", article.FailedExpectation());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(5, article.Total().Traces());
        Assert.Equal(6, article.Total().PassedExpectations());
        Assert.Equal(18, article.ShrinkCount);
        Assert.Equal(7, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Coach", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Coach Name", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("Coach-3", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(10, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Course", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Course Name", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("Course-1", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal(14, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Assign Coach to Course", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Coach Name", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("Coach-3", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal("Course Name", article.Execution(3).Trace(2).Read().Label);
        Assert.Equal("Course-1", article.Execution(3).Trace(2).Read().Value);
        Assert.Equal(15, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Update Course Skills", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Course Skills", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("[ _ ]", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Course Name", article.Execution(4).Trace(1).Read().Label);
        Assert.Equal("Course-1", article.Execution(4).Trace(1).Read().Value);
        Assert.Equal("Assigned Coach Must Be Suitable", article.PassedExpectation(1).Read().Label);
        Assert.Equal(14, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("Coach Name is set", article.PassedExpectation(2).Read().Label);
        Assert.Equal(5, article.PassedExpectation(2).Read().TimesPassed);
        Assert.Equal("Course Name Set", article.PassedExpectation(3).Read().Label);
        Assert.Equal(1, article.PassedExpectation(3).Read().TimesPassed);
        Assert.Equal("Coach is assigned", article.PassedExpectation(4).Read().Label);
        Assert.Equal(3, article.PassedExpectation(4).Read().TimesPassed);
        Assert.Equal("Coach Skills Set", article.PassedExpectation(5).Read().Label);
        Assert.Equal(1, article.PassedExpectation(5).Read().TimesPassed);
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
        from trace in Checkr.Trace("Coach Name", () => name)
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.ToPool("Store Coach", coach) // Store this coach in the pool.
        select Case.Closed;

    [CodeExample]
    private static readonly Func<Coach, CheckrOf<Case>> UpdateCoachSkills =
        coach =>
            from skills in Checkr.Input("Coach Skills",
                Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
            from trace in Checkr.Trace("Coach Name", () => coach.Name)
            from updateSkills in Checkr.Act("Update Coach Skills",
                () => coach.UpdateSkills(skills))
            from checkSkills in Checkr.Expect("Coach Skills Set",
                () => coach.Skills.Count
                   == skills.Count() && coach.Skills.All(skills.Contains))
            select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name",
            from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
        from trace in Checkr.Trace("Course Name", () => name)
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course) // Store this course in the pool.
        select Case.Closed;

    [CodeExample]
    private static readonly Func<Course, CheckrOf<Case>> UpdateCourseSkills =
        // Pick a random course from the pool.
        course =>
            from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
            from traceCourse in Checkr.Trace("Course Name", () => course.Name)
            from updateSkills in Checkr.Act("Update Course Skills",
                () => course.UpdateRequiredSkills(skills))          // Update it.
            from checkSkills in Checkr.Expect("Course Required Skills Set",
                () => course.RequiredSkills.Count
                   == skills.Count() && course.RequiredSkills.All(skills.Contains))
            select Case.Closed;

    [CodeExample]
    private static readonly Func<Coach, Course, CheckrOf<Case>> AssignCoachToCourse =
        (coach, course) =>
            from traceCoach in Checkr.Trace("Coach Name", () => coach.Name)
            from traceCourse in Checkr.Trace("Course Name", () => course.Name)
            from assign in Checkr.Act("Assign Coach to Course", () => course.AssignCoach(coach))
            from isAssigned in Checkr.Expect("Coach is assigned", () => course.AssignedCoach == coach)
            select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from coaches in Trackr.Pool<Coach>() // Retrieve the coaches pool.
        from assignedCheck in coaches.ExpectForEachWhen("Assigned Coach Must Be Suitable",
            coach => coach.AssignedCourses.Any(), // Check that all assigned coaches are suitable.
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
        select Case.Closed;

    [CodeExample]
    public static readonly CheckrOf<Case> TheCheckr =
        // Create a pool of coaches.
        from coaches in Trackr.PoolFor<Coach>()
        // Create a pool of courses.          
        from courses in Trackr.PoolFor<Course>()
        from operations in Checkr.OneOfWhen(
            // Create up to 10 coaches.
            coaches.When(a => a.Count() < 10, CreateCoach),
            // Maybe update existing coaches. Pick a random coach from the pool.
            coaches.When(a => a.Any(), "Coach to Update Skills", UpdateCoachSkills),
            // Create up to 5 courses.
            courses.When(a => a.Count() < 5, CreateCourse),
            // Maybe update existing courses.
            courses.When(a => a.Any(), "Course to Update Skills", UpdateCourseSkills),
            // Maybe assign coach if possible.
            // Pick a random coach and course, if there exists a combination for which the predicate holds.
            Pool.These(coaches, courses).When("Assign Coach to Course",
                (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
            // Ensure the system invariant is met at all times.
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;
}