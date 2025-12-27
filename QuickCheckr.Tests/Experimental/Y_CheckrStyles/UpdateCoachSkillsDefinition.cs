using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Y_CheckrStyles;

[CodeExample]
public class UpdateCoachSkillsDefinition
{
    public static readonly Func<PoolElement<Coach>, CheckrOf<Case>> GetCheckr =
        coach =>
            from trace in Trace(coach)
            from skills in GetSkills()
            from act in UpdateCoachSkills(coach, skills)
            from verify in Verify(coach, skills)
            select Case.Closed;

    private static CheckrOf<Case> Trace(PoolElement<Coach> coach) =>
        Checkr.Trace("Coach Name", () => coach.Value.Name);

    private static CheckrOf<IEnumerable<string>> GetSkills() =>
        Checkr.Input("Coach Skills", Fuzzr.OneOf(Skills.List).Unique(Guid.NewGuid()).Many(3, 5));

    private static CheckrOf<Case> UpdateCoachSkills(PoolElement<Coach> coach, IEnumerable<string> skills) =>
        Checkr.Act("Update Coach Skills", () => coach.Value.UpdateSkills(skills));

    private static CheckrOf<Case> Verify(PoolElement<Coach> coach, IEnumerable<string> skills) =>
        Checkr.Expect("Coach Skills Set", () => SkillsMatch(coach, skills));

    private static bool SkillsMatch(PoolElement<Coach> coach, IEnumerable<string> skills)
        => coach.Value.Skills.Count == skills.Count() && coach.Value.Skills.All(skills.Contains);
}
