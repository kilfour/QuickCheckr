using QuickFuzzr;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

public static class SkillFuzzr
{
    public static readonly FuzzrOf<IEnumerable<string>> Many =
        Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3);
}