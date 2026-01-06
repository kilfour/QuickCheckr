using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickFuzzr;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

public class GatheringEvidenceCheckr
{
    public static CheckrOf<Case> TheCheckr() =>
        from coaches in Trackr.PoolFor<Coach>()
        from courses in Trackr.PoolFor<Course>()
        from operations in Checkr.OneOfWhen(
            Trackr.PoolWhen<Coach>(a => a.Count < 10, CreateCoach),
            Trackr.PoolWhen("Coach to Update Skills", a => a.Count != 0, UpdateCoachSkills),
            Trackr.PoolWhen<Course>(a => a.Count < 5, CreateCourse),
            Trackr.PoolWhen("Course to Update Skills", a => a.Count != 0, UpdateCourseSkills),
            Trackr.PoolWhen("(Coach, Course) to Assign",
                (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;

    private static readonly CheckrOf<Case> CreateCoach =
        from name in Checkr.Input("Coach Name", NameFuzzr.For("Coach"), Shrink.None<string>)
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.ToPool("Store Coach", coach)
        select Case.Closed;

    private static readonly Func<PoolElement<Coach>, CheckrOf<Case>> UpdateCoachSkills =
        coach =>
            from skills in Checkr.Input("Coach Skills"
                , Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3)
                , () => [
                    Shrink.Default<IEnumerable<string>>(),
                    Shrink.Collection([[.. coach.Value.Skills]]) ]
                , Reduce.Values<IEnumerable<string>>([[], [.. coach.Value.Skills]]))
            from updateSkills in Checkr.Act("Update Coach Skills",
                () => coach.Value.UpdateSkills(skills))
            from checkSkills in Checkr.Expect("Coach Skills Set",
                () => coach.Value.Skills.Count
                   == skills.Count() && coach.Value.Skills.All(skills.Contains))
            select Case.Closed;

    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name", NameFuzzr.For("Course"), Shrink.None<string>)
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course)
        select Case.Closed;

    private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
        course =>
            from skills in Checkr.Input("Course Skills",
                Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            from updateSkills in Checkr.Act("Update Course Skills",
                () => course.Value.UpdateRequiredSkills(skills))
            from checkSkills in Checkr.Expect("Course Required Skills Set",
                () => course.Value.RequiredSkills.Count
                   == skills.Count() && course.Value.RequiredSkills.All(skills.Contains))
            select Case.Closed;

    private static readonly Func<PoolElement<Coach>, PoolElement<Course>, CheckrOf<Case>> AssignCoachToCourse =
        (coach, course) =>
            from assign in Checkr.Act("Assign Coach to Course",
                () => course.Value.AssignCoach(coach.Value))
            from isAssigned in Checkr.Expect("Coach is assigned",
                () => course.Value.AssignedCoach == coach.Value)
            from hasCourse in Checkr.Expect("Coach has Course",
                () => coach.Value.AssignedCourses.Contains(course.Value))
            select Case.Closed;

    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from assignedCheck in Trackr.PoolExpectEachWhen<Coach>("Assigned Coach Must Be Suitable",
            coach => coach.AssignedCourses.Any(),
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
        select Case.Closed;
}
