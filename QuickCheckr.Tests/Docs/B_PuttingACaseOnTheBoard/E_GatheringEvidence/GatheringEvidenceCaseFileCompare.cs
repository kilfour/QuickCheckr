using QuickCheckr.EvidenceGathering;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Explains;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

[DocFileHeader("")]
public class GatheringEvidenceCaseFileCompare
{
    [Fact]
    [DocContent("**Casefiles are considered *not similar* when:**")]
    [DocContent("- *Failure Description* is not equal.")]
    public void CaseFileNotSimilar_FailureDescription()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { FailingExpectation = "Some Failure" }));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { FailingExpectation = "Other Failure" }));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("- *Execution Count* is not equal.")]
    public void CaseFileNotSimilar_ExecutionCount()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { ExecutionCount = 1 }));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { ExecutionCount = 2 }));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("- Any **Execution** differs, like so:")]
    public void CaseFileNotSimilar_TimesExecuted()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Number of *Actions Performed* is not equal.")]
    public void CaseFileNotSimilar_NumberOfActions()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did"))
                .AddActionDeposition(new ActionDeposition("Something Else I Did"))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Any *Action label* is not equal.")]
    public void CaseFileNotSimilar_ActionLabels()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something Else I Did"))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Number of *Inputs* is not equal.")]
    public void CaseFileNotSimilar_Inputs()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Any *Input label* is not equal.")]
    public void CaseFileNotSimilar_InputLabels()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Other Label", 0))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
    [Fact]
    [DocContent("\nPer default, the order of Executions does not matter for similarity.")]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatter()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did")))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other)); // <= should not matter
    }

    [Fact]
    [DocContent("An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.")]
    public void CaseFileNotSimilar_ExecutionOrderDoesMatter()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition(true, "Label", 0))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
}