using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Verification;

public class DistinctionTests
{
    [Fact]
    public void InlineDistinctionsProduceSeparateFailureCategories()
    {
        var distinctions = new Queue<bool>([false, true]);
        var currentDistinction = false;
        var distinctionCalls = 0;
        CheckrOf<Case> check = state =>
        {
            if (!state.EvidenceRecorder.IsReplay)
                currentDistinction = distinctions.Dequeue();

            Checkr.Distinguish(() =>
            {
                distinctionCalls++;
                return currentDistinction;
            })(state);
            return Checkr.Expect("always fails", () => false)(state);
        };
        var custodian = new InMemoryCustodian([]);
        var configured = check.Configure(config => config with
        {
            FileAs = "distinctions",
            Custodian = custodian,
            ShrinkMode = ShrinkMode.None
        });

        var exception = Assert.Throws<FalsifiableException>(() =>
            configured.Conduct(1.Investigations(), 2.Runs(), 1.ExecutionsPerRun()));

        var inquiry = Assert.IsType<Inquiry>(exception.TheCaseFile);
        Assert.Equal(2, inquiry.DistinctFailureCount);
        Assert.Equal(2, distinctionCalls);
    }
}
