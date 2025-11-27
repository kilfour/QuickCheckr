using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.B_BuildingBlocks.T_TrackrProvenWhen;

[DocFile]
public class TrackrProvenWhen : BaseTest
{
    private int executionCount = 0;
    [Fact]
    [DocExample(typeof(TrackrProvenWhen), nameof(GetCheckr))]
    public void RunTest()
    {
        GetCheckr().Run(100.ExecutionsPerRun(), a => a with { Seed = null });
        Assert.Equal(3, executionCount);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Act("hack for asserting execution count", () => ++executionCount)
            from spec in Trackr.ProvenWhen("Counter equals 3", () => counter == 3)
            from shouldNeverFail in Checkr.Spec("Should never fail, because of ProvenWhen", () => counter <= 3)
            select Case.Closed;
    }
}