using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

[DocFileHeader("Show, Don't Tell")]
[DocContent(
@"As stated in [the guide][Guide] we're going to use the [Bolting Horses][BoltingHorses] example,
to demonstrate how to find all the different ways the invariant can be violated.

So we're using the same model, and *almost* exactly the same Checkr.  
Changes:
- Added Shrink.None to the Name inputs. 
- Changed the `UpdateCoachSkills` a little to avoid the issue documented [here][J_TheCaseOfTheInvisibleList]:")]
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
        GatheringEvidenceCheckr.TheCheckr().GatherEvidence(100.Investigations(), 3.Runs(), 20.ExecutionsPerRun(),
            cfg => cfg with
            {
                FileAs = "GatheringEvidence",
                ReportMode = cfg.ReportMode & ~ReportMode.Labels,
                Custodian = Custodian.Local()
            });
    }
}