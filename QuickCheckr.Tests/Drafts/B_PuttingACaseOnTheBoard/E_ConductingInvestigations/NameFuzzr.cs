using QuickCheckr;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_ConductingInvestigations;

public static class NameFuzzr
{
    public static FuzzrOf<string> For(string entity) =>
        from cnt in Fuzzr.Counter($"{entity}-name")
        select $"{entity}-{cnt}";
}