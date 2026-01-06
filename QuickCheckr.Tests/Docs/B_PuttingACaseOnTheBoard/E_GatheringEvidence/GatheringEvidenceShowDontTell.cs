using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
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
[DocCodeFile("GatheringEvidenceCheckr.cs", "csharp", 6)]
[DocContent(
@"Running the Checkr using the code snippet at the top of this file, consistently results in five different reports.")]
[DocContent(
@"For the code on display here, running this checkr using the code snippet at the top of this file, 
consistently results in two different reports.")]
[DocCodeFile("report1.qr", "text")]
[DocCodeFile("report2.qr", "text")]
public class GatheringEvidenceShowDontTell : BaseTest
{
    [Fact]//(Skip = "Explicit: Touches the File System.")]
    public void RunTest()
    {
        GatheringEvidenceCheckr.TheCheckr().GatherEvidence(100.Investigations(), 3.Runs(), 50.ExecutionsPerRun(),
            cfg => cfg with
            {
                FileAs = "GatheringEvidence",
                // DeliberationPolicy = DeliberationPolicy.MinimumShrinkCount,
                ReportMode = cfg.ReportMode & ~ReportMode.Labels,
                Custodian = Custodian.Local()
            });
    }

    [Fact(Skip = "DEBUG")]
    public void Debug()
    {
        var article = TheJournalist.Exposes(() =>
            GatheringEvidenceCheckr.TheCheckr().Run(1772714234, 20.ExecutionsPerRun(),
                cfg => cfg with { InternalAffairs = [] }));
        // cfg => cfg with { InternalAffairs = [InternalAffairs.InputShrinking] }));
        WriteReport<GatheringEvidenceShowDontTell>(article, true);
    }
}