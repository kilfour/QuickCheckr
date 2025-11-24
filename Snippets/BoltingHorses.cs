using QuickCheckr.Tests.Notes.T_Examples.E_HorsesAgain.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;

namespace QuickCheckr.Tests.Notes.T_Examples.E_HorsesAgain;

public class BoltingHorses : BaseTest
{
    [Fact]
    public void CheckIt()
    {
        var check =
            from coaches in Trackr.Stashed(() => new Dictionary<int, Coach>())
            from courses in Trackr.Stashed(() => new Dictionary<int, Course>())
            from operations in Checkr.OneOfIf(
                (() => coaches.Count < 10, CreateCoach(coaches)),
                (() => coaches.Count > 0, UpdateCoachSkills(coaches)),
                (() => courses.Count < 5, CreateCourse(courses)),
                (() => courses.Count > 0, UpdateCourseSkills(courses)),
                (() => coaches.Count > 0 && courses.Values.Any(course => course.AssignedCoach == null),
                    AssignCoachToCourse(coaches, courses)))
            from invariant in AssignedCoachesMustBeSuitable(coaches)
            select Case.Closed;
        var article = TheJournalist.Exposes(() => check.Run(20.ExecutionsPerRun(), a => a with { Seed = 13918324 }));
        WriteAsserts(article, true);
        WriteReport(article, "BoltingHorses.txt", true);
    }

    private static CheckrOf<Case> CreateCoach(Dictionary<int, Coach> coaches) =>
        from id in Checkr.Input("Id", Fuzzr.Int().Unique("coach-id"))
        from name in Checkr.Input("Coach Name", Fuzzr.String().Unique("coach-name"))
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Spec("Coach Name is set", () => coach.Name == name)
        from store in Trackr.Execute(() => coaches[id] = coach)
        select Case.Closed;

    private static CheckrOf<Case> UpdateCoachSkills(Dictionary<int, Coach> coaches) =>
        from coachId in Checkr.Input("Coach Id", Fuzzr.OneOf(coaches.Keys.AsEnumerable()))
        from coach in Trackr.Execute(() => { if (coaches.TryGetValue(coachId, out Coach? value)) return value; return null; })
        from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.ActIf("Update Coach Skills",
            () => coach is not null,
            () => coach.UpdateSkills(skills))
        from checkSkills in Checkr.SpecIf("Coach Skills Set",
            () => coach is not null,
            () => coach.Skills.Count == skills.Count() && coach.Skills.All(skills.Contains))
        select Case.Closed;

    private static CheckrOf<Case> CreateCourse(Dictionary<int, Course> courses) =>
        from id in Checkr.Input("Id", Fuzzr.Int().Unique("course-id"))
        from name in Checkr.Input("Course Name", Fuzzr.String().Unique("course-name"))
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Spec("Course Name Set", () => course.Name == name)
        from store in Trackr.Execute(() => courses[id] = course)
        select Case.Closed;

    private static CheckrOf<Case> UpdateCourseSkills(Dictionary<int, Course> courses) =>
        from courseId in Checkr.Input("Course Id", Fuzzr.OneOf(courses.Keys.AsEnumerable()))
        from course in Trackr.Execute(() => { if (courses.TryGetValue(courseId, out Course? value)) return value; return null; })
        from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
        from updateSkills in Checkr.ActIf("Update Course Skills",
            () => course is not null,
            () => course.UpdateRequiredSkills(skills))
        from checkSkills in Checkr.SpecIf("Course Required Skills Set",
            () => course is not null,
            () => course.RequiredSkills.Count == skills.Count() && course.RequiredSkills.All(skills.Contains))
        select Case.Closed;

    private static CheckrOf<Case> AssignCoachToCourse(
            Dictionary<int, Coach> coaches,
            Dictionary<int, Course> courses) =>
        from coachId in Checkr.Input("Coach Id", Fuzzr.OneOf(coaches.Keys.AsEnumerable()))
        from coach in Trackr.Execute(() => { if (coaches.TryGetValue(coachId, out Coach? value)) return value; return null; })
        from courseId in Checkr.Input("Course Id", Fuzzr.OneOf(courses.Keys.AsEnumerable()))
        from course in Trackr.Execute(() => { if (courses.TryGetValue(courseId, out Course? value)) return value; return null; })
        from assign in Checkr.ActIf("Assign Coach to Course",
            () => coach != null && course != null && coach.IsSuitableFor(course),
            () => course.AssignCoach(coach))
        from isAssigned in Checkr.SpecIf("Coach is assigned",
            () => coach != null && course != null && coach.IsSuitableFor(course),
            () => course.AssignedCoach == coach)
        select Case.Closed;

    private static CheckrOf<Case> AssignedCoachesMustBeSuitable(Dictionary<int, Coach> coaches) =>
        Checkr.SpecIf("AssignedCoachesMustBeSuitable",
            () => coaches.Values.Any(coach => coach.AssignedCourses.Any()),
            () => coaches.Values.All(
                coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course))));

    private static readonly string[] SkillPool =
   {
        "TDD", "Refactoring", "C#", "ASP.NET", "EF Core", "SQL",
        "DomainDrivenDesign", "UnitTesting", "Git", "CI/CD",
        "JavaScript", "React", "Elm", "Architecture"
    };
}