using QuickCheckr;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

public static class NameFuzzr
{
    public static FuzzrOf<string> For(string entity) =>
        from cnt in Fuzzr.Counter($"{entity}-name")
        select $"{entity}-{cnt}";
}