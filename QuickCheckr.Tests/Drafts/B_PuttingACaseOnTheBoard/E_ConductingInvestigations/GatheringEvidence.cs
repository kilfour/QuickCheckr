using QuickCheckr.Tests.Docs.A_Guide;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;
using QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_ConductingInvestigations;

[DocFile]
[DocLink(typeof(PuttingACaseOnTheBoard))]
[DocLink(typeof(Guide))]
[DocLink(typeof(BoltingHorses))]
[DocLink(typeof(J_TheCaseOfTheInvisibleCollection))]
[DocContent("[The Board][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault] \n")]
[DocContent("This is another way to run a Checkr, and an alternate method to populate the *Evidence Vault* with seeds for failing runs.")]
[DocHeader("Running Investigations")]
[DocExample(typeof(GatheringEvidence), nameof(RunGatherEvidence))]
[DocContent(
@"When you use the above method the following steps are taken:
- An *investigation* requires`FileAs`. If this is not configured it throws a `VaultNotConfiguredException`.
- The existing evidence from the vault is loaded (if available), the seeds are run, and the failing casefiles are kept in memory.
- Next we run the specified number of Investigations, and any new failing case file is added to the in-memory list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- And finally, all evidence and reports are saved to the vault.

**Note:** An *Investigation* is a new kind of scope.  
Running a Checkr in the *normal* way will always produce one report, even if it declares deliberation.
When you're gathering evidence, a report is generated per Investigation. This allows for finding different bugs in the same system,
or for documenting the different ways a single bug can manifest.

As stated above, similar CaseFiles are not reported in order to increase the signal to noise ratio.  
")]
[DocInclude(typeof(GatheringEvidenceCaseFileCompare), true)]
[DocInclude(typeof(GatheringEvidenceDistinguish))]
[DocInclude(typeof(GatheringEvidenceShowDontTell))]
[DocInclude(typeof(GatheringEvidenceObservation))]
public class GatheringEvidence
{
    [CodeSnippet]
    private static void RunGatherEvidence(CheckrOf<Case> checkr) =>
        checkr
            .Configure(cfg => cfg with { FileAs = "GatheringEvidence" })
            .Conduct(50.Investigations(), 3.Runs(), 10.ExecutionsPerRun());

    [Fact]
    public void RunTest()
    {
        Assert.Throws<VaultNotConfiguredException>(
            () => GatheringEvidenceCheckr.TheCheckr()
                .Configure(a => a)
                .Conduct(1.Investigations(), 1.Runs(), 1.ExecutionsPerRun()));
    }

}
