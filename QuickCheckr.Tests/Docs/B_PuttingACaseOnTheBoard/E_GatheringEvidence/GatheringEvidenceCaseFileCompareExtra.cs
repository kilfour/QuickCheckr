using QuickCheckr.EvidenceGathering;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

public class GatheringEvidenceCaseFileCompareExtra
{
    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatterInputs()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0, Maybe.Nothing(), Maybe.Nothing())))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition(true, "Label", 0, Maybe.Nothing(), Maybe.Nothing()))));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other)); // <= should not matter
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatterCollapsed()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0, Maybe.Nothing(), Maybe.Nothing())))
            .AddExecutionDeposition(new ExecutionDeposition(2))
            .AddExecutionDeposition(new ExecutionDeposition(3)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition(true, "Label", 0, Maybe.Nothing(), Maybe.Nothing())))
            .AddExecutionDeposition(new ExecutionDeposition(3)));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other)); // <= should not matter
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesMatter_Doubly()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 2, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 3, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 4, Maybe.Nothing(), Maybe.Nothing()))));

        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 2, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 3, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 4, Maybe.Nothing(), Maybe.Nothing()))));

        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesMatter_Doubly_Collapsed()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 2, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 2, Maybe.Nothing(), Maybe.Nothing()))));

        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 3, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 4, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 5, Maybe.Nothing(), Maybe.Nothing())))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 6, Maybe.Nothing(), Maybe.Nothing()))));

        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
}