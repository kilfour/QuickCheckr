using QuickCheckr.EvidenceGathering;
using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

[DocFileHeader("")]
public class GatheringEvidenceCaseFileCompare
{
    [Fact]
    [DocContent("**Casefiles are considered *not similar* when:**")]
    [DocContent("- *Failure Description* is not equal.")]
    public void CaseFileNotSimilar_FailureDescription()
    {
        var caseFile = CaseFile.From(new FailureInfo(FailingExpectation: new ExpectationFailure("Some Failure", [])), new RunInfo());
        var other = CaseFile.From(new FailureInfo(FailingExpectation: new ExpectationFailure("Other Failure", [])), new RunInfo());
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("- *Execution Count* is not equal.")]
    public void CaseFileNotSimilar_ExecutionCount()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("- Any **Execution** differs, like so:")]
    public void CaseFileNotSimilar_TimesExecuted()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Something I Did")));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Number of *Actions Performed* is not equal.")]
    public void CaseFileNotSimilar_NumberOfActions()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did")));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did"))
                .AddActionDeposition(new ActionDeposition("Something Else I Did")));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Any *Action label* is not equal.")]
    public void CaseFileNotSimilar_ActionLabels()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did")));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something Else I Did")));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Number of *Inputs* is not equal.")]
    public void CaseFileNotSimilar_Inputs()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Any *Input label* is not equal.")]
    public void CaseFileNotSimilar_InputLabels()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Other Label", 0)));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
    [Fact]
    [DocContent("\nPer default, the order of Executions does not matter for similarity.")]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatter()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did")))
            .AddExecutionDeposition(new ExecutionDeposition(2));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Something I Did")));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other, false)); // <= should not matter
    }

    [Fact]
    [DocContent("An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.")]
    public void CaseFileNotSimilar_ExecutionOrderDoesMatter()
    {
        var caseFile = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(2));
        var other = CaseFile.From(new FailureInfo(), new RunInfo())
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition(true, "Label", 0)));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
}