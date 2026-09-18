using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_ConductingInvestigations;

[DocFileHeader("Show, Don't Tell")]
[DocContent(
@"As stated in [the guide][Guide] we're going to use the [Bolting Horses][BoltingHorses] example,
to demonstrate how to find all the different ways the invariant can be violated.

So we're using the same model, and *almost* exactly the same Checkr.  
Changes:
- Added Shrink.None to the Name inputs. 
- Updated `UpdateCoachSkills` to avoid the issue documented [here][J_TheCaseOfTheInvisibleCollection]:")]
[DocCodeFile("GatheringEvidenceCheckr.cs", "csharp", 4)]
[DocContent(
@"For the code on display here, running this checkr using the code snippet at the top of this doc, 
consistently results in two different reports.")]
[DocCodeFile("report1.qr", "text")]
[DocCodeFile("report2.qr", "text")]
public class GatheringEvidenceShowDontTell : BaseTest
{
    [Fact(Skip = "Touches the file system.")]
    public void RunTest()
    {
        GatheringEvidenceCheckr.TheCheckr()
            .Configure(cfg => cfg with
            {
                FileAs = "GatheringEvidence",
                Custodian = Custodian.Local()
            })
            .Conduct(100.Investigations(), 3.Runs(), 20.ExecutionsPerRun());
    }
}