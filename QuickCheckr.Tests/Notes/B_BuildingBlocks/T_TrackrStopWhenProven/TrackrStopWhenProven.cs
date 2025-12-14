using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.B_BuildingBlocks.T_TrackrStopWhenProven;

[DocFile]
public class TrackrStopWhenProven : BaseTest
{
    private int executionCount = 0;
    [Fact]
    [DocExample(typeof(TrackrStopWhenProven), nameof(GetCheckr))]
    public void RunTest()
    {
        GetCheckr().Run(100.ExecutionsPerRun());
        Assert.Equal(3, executionCount);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Act("hack for asserting execution count", () => ++executionCount)
            from stop in Trackr.StopWhenProven("Counter equals 3", () => counter == 3)
            from shouldNeverFail in Checkr.Expect("Should never fail, because of StopWhenProven", () => counter <= 3)
            select Case.Closed;
    }
}