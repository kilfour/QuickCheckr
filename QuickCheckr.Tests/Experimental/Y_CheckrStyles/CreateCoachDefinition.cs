using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Y_CheckrStyles;

[CodeExample]
public class CreateCoachDefinition
{
    public static readonly CheckrOf<Case> GetCheckr =
        from name in Name()
        from trace in Trace(name)
        from coach in CreateCoach(name)
        from verify in Verify(coach, name)
        select Case.Closed;

    private static readonly FuzzrOf<string> CoachNameFuzzr =
        from cnt in Fuzzr.Counter("coach-name")
        select $"Coach-{cnt}";

    private static CheckrOf<string> Name() =>
        Checkr.Input("Coach Name", CoachNameFuzzr);

    private static CheckrOf<Case> Trace(string name) =>
        Checkr.Trace("Coach Name", () => name);

    private static CheckrOf<Coach> CreateCoach(string name) =>
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from toPool in Trackr.ToPool("Store Coach", coach)
        select coach;

    private static CheckrOf<Case> Verify(Coach coach, string name) =>
        Checkr.Expect("Coach Name is set", () => coach.Name == name);
}
