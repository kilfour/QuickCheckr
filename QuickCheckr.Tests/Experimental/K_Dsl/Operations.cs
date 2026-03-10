using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickCheckr.Tests.Experimental.K_Dsl.Dsl;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.K_Dsl;


[CodeExample]
public class CreateCoach : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
        system.Create<Coach>()
            .When(coaches => coaches.Count <= 10)
            .With(from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
            .Perform(input => new Coach(input))
            .Expect(("Name", (coach, input) => coach.Name == input));
}

[CodeExample]
public class UpdateCoachSkills : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
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
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
        system.Create<Course>()
            .When(courses => courses.Count <= 5)
            .With(from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
            .Perform(input => new Course(input))
            .Expect(("Name", (course, input) => course.Name == input));
}

[CodeExample]
public class UpdateCourseSkills : Operation
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
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
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest system) =>
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