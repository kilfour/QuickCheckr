using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickFuzzr;

namespace QuickCheckr.Tests.Experimental.Y_CheckrStyles;

public class Refactored
{
    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name",
            from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
        from trace in Checkr.Trace("Course Name", () => name)
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course)
        select Case.Closed;

    private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
        course =>
            from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(Skills.List).Unique(Guid.NewGuid()).Many(3, 5))
            from traceCourse in Checkr.Trace("Course Name", () => course.Value.Name)
            from updateSkills in Checkr.Act("Update Course Skills",
                () => course.Value.UpdateRequiredSkills(skills)) // Update the Course.
            from checkSkills in Checkr.Expect("Course Required Skills Set",
                () => course.Value.RequiredSkills.Count
                   == skills.Count() && course.Value.RequiredSkills.All(skills.Contains))
            select Case.Closed;

    private static readonly Func<Coach, Course, CheckrOf<Case>> AssignCoachToCourse =
        (coach, course) =>
            from traceCoach in Checkr.Trace("Coach Name", () => coach.Name)
            from traceCourse in Checkr.Trace("Course Name", () => course.Name)
            from assign in Checkr.Act("Assign Coach to Course", () => course.AssignCoach(coach))
            from isAssigned in Checkr.Expect("Coach is assigned", () => course.AssignedCoach == coach)
            select Case.Closed;

    private static CheckrOf<Case> AssignedCoachesMustBeSuitable(Pool<Coach> coaches) =>
        from assignedCheck in coaches.ExpectForEachWhen("Assigned Coach Must Be Suitable",
            coach => coach.AssignedCourses.Any(),
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
        select Case.Closed;

    public static readonly CheckrOf<Case> TheCheckr =
        from coaches in Trackr.PoolFor<Coach>()
        from courses in Trackr.PoolFor<Course>()
        from operations in Checkr.OneOfWhen(
            coaches.When(a => a.Count() < 10, CreateCoachDefinition.GetCheckr),
            coaches.When(a => a.Any(), "Coach to Update Skills", UpdateCoachSkillsDefinition.GetCheckr),
            courses.When(a => a.Count() < 5, CreateCourse),
            courses.When(a => a.Any(), "Course to Update Skills", UpdateCourseSkills),
            Pool.These(coaches, courses).When("Assign Coach to Course",
                (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
        from invariant in AssignedCoachesMustBeSuitable(coaches)
        select Case.Closed;
}