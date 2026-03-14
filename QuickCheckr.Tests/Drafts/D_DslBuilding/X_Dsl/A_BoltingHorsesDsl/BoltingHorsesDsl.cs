using QuickCheckr;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.A_BoltingHorsesDsl;

[DocFile]
public class BoltingHorsesDsl : InvariantsTest<BoltingHorsesDsl>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocInvariantHeader]
    [DocInvariant]
    [DocExample(typeof(CreateCoach))]
    [DocExample(typeof(UpdateCoachSkills))]
    [DocExample(typeof(CreateCourse))]
    [DocExample(typeof(UpdateCourseSkills))]
    [DocExample(typeof(AssignCoachToCourse))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    protected override CaseFile TheInvariant() =>
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


    [CodeExample]
    public class CreateCoach : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Create<Coach>()
                .When(coaches => coaches.Count <= 10)
                .With(from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
                .Perform(input => new Coach(input))
                .Expect(("Name", (coach, input) => coach.Name == input));
    }

    [CodeExample]
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

    [CodeExample]
    public class CreateCourse : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Create<Course>()
                .When(courses => courses.Count <= 5)
                .With(from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
                .Perform(input => new Course(input))
                .Expect(("Name", (course, input) => course.Name == input));
    }

    [CodeExample]
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

    [CodeExample]
    public class AssignCoachToCourse : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Update<Coach, Course>("Assign")
                .When((coach, course) => coach.IsSuitableFor(course))
                .Perform((coach, course) => course.AssignCoach(coach))
                .Expect(("Assigned", (coach, course) => course.AssignedCoach == coach));
    }

    public class Skills
    {
        public static readonly string[] Pool =
        [
            "TDD", "Refactoring", "C#", "ASP.NET", "EF Core", "SQL",
        "DomainDrivenDesign", "UnitTesting", "Git", "CI/CD",
        "JavaScript", "React", "Elm", "Architecture"
        ];
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("All Assigned Coaches Must Be Suitable", article.FailureDescription());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(4, article.Total().PoolTraces());
        Assert.Equal(6, article.Total().PassedExpectations());
        Assert.Equal(14, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Course", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("'Create Course' to Pool", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Course-1", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(4, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Coach", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("'Create Coach' to Pool", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("Coach-1", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal(10, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Update (Coach, Course) Assign", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Update (Coach, Course) Assign", article.Execution(3).PoolTrace(1).Read().Label);
        Assert.Equal("( Coach-1, Course-1 )", article.Execution(3).PoolTrace(1).Read().Value);
        Assert.Equal(13, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Update Course Skills", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("'Update Course Skills' Input", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("[ _ ]", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Update Course Skills", article.Execution(4).PoolTrace(1).Read().Label);
        Assert.Equal("Course-1", article.Execution(4).PoolTrace(1).Read().Value);
        Assert.Equal("'Create Course' Name", article.PassedExpectation(1).Read().Label);
        Assert.Equal(4, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("All Assigned Coaches Must Be Suitable", article.PassedExpectation(2).Read().Label);
        Assert.Equal(12, article.PassedExpectation(2).Read().TimesPassed);
        Assert.Equal("'Create Coach' Name", article.PassedExpectation(3).Read().Label);
        Assert.Equal(4, article.PassedExpectation(3).Read().TimesPassed);
        Assert.Equal("'Update Coach Skills' Skills", article.PassedExpectation(4).Read().Label);
        Assert.Equal(2, article.PassedExpectation(4).Read().TimesPassed);
        Assert.Equal("'Update (Coach, Course) Assign' Assigned", article.PassedExpectation(5).Read().Label);
        Assert.Equal(2, article.PassedExpectation(5).Read().TimesPassed);
        Assert.Equal("'Update Course Skills' Skills", article.PassedExpectation(6).Read().Label);
        Assert.Equal(1, article.PassedExpectation(6).Read().TimesPassed);
    }
}