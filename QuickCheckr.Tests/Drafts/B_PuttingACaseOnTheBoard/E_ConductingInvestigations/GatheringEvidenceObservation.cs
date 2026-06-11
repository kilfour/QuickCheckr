using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

[DocFileHeader("An Observation")]
[DocContent(
@"When I first implemented this feature, I was quite puzzled.
Surely there are more ways this Checkr can fail, I thought.  

It took a bit of investigating as to why this is not the case. Let me give you an example of what is going on.

If you look at the second report, the one with 6 executions, surely there is a similar case,
but instead of updating the Coach skills, we update the Course skills and that would also fail.
Now that's a new way of failing the Checkr, right?

Wrong!  

Below is the report of the other failure (the one with 4 executions) with shrinking disabled,
but I'll save you the *bit of investigating* and give you a recap:
- Execution 03: Create Course-1
- Execution 06: Create Coach-3
- Execution 07: Update Coach Skills to [ ""DomainDrivenDesign"", ""JavaScript"" ]
- Execution 08: Update Course Skills to [ ""DomainDrivenDesign"" ]
- Execution 09: Assign Coach-3 to Course-1
- Execution 10: Update Course Skills to [ ""JavaScript"", ""TDD"", ""CI/CD"" ]

As you can see, this is exactly the case described above.  
The Shrinker however managed to collapse that one down to only four executions, so it is in fact covered by the first report.
")]
[DocCodeFile("GatheringEvidenceObservation.txt", "text")]
public class GatheringEvidenceObservation : BaseTest
{
    [Fact(Skip = "Explicit: Touches the File System.")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GatheringEvidenceCheckr.TheCheckr().Configure(a => a with { ShrinkMode = ShrinkMode.None }).Run(1652806413, 20.ExecutionsPerRun()));
        WriteReport(article, "GatheringEvidenceObservation.txt", true);
    }
}