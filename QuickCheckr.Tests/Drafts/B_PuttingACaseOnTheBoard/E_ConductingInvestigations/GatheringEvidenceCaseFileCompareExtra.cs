using QuickCheckr.EvidenceGathering;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

public class GatheringEvidenceCaseFileCompareExtra
{
    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatterInputs()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(2));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false)); // <= should not matter
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatterCollapsed()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(2))
            .AddExecutionDeposition(new ExecutionDeposition(3));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(3));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false)); // <= should not matter
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesMatter_Doubly()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 2)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 3)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 4)));

        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 2)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 3)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 4)));

        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesMatter_Doubly_Collapsed()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 2)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 2)));

        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 3)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 4)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition(true, "Input One", 5)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition(true, "Input Two", 6)));

        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
}