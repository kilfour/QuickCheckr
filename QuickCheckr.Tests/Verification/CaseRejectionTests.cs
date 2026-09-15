using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Verification;

public class CaseRejectionTests
{
    [Fact]
    public void AnyInlineRejectionExcludesTheFailingCase()
    {
        var rejectionCalls = 0;
        CheckrOf<Case> check =
            from keep in Checkr.RejectCaseWhen(() =>
            {
                rejectionCalls++;
                return false;
            })
            from reject in Checkr.RejectCaseWhen(() =>
            {
                rejectionCalls++;
                return true;
            })
            from expectation in Checkr.Expect("always fails", () => false)
            select Case.Closed;

        check.Run();

        Assert.Equal(2, rejectionCalls);
    }

    [Fact]
    public void RejectionContinuesSearchingForAnAcceptedFailure()
    {
        var values = new Queue<int>([1, 2]);
        var currentValue = 0;
        CheckrOf<Case> check = async state =>
        {
            if (!state.EvidenceRecorder.IsReplay)
                currentValue = values.Dequeue();

            await Checkr.RejectCaseWhen(() => currentValue == 1)(state).ConfigureAwait(false);
            await Checkr.Distinguish(() => currentValue)(state).ConfigureAwait(false);
            return await Checkr.Expect("always fails", () => false)(state).ConfigureAwait(false);
        };

        var exception = Assert.Throws<FalsifiableException>(() => check.Run(2.Runs()));

        var caseFile = Assert.IsType<CaseFile>(exception.TheCaseFile);
        Assert.Equal(2, Assert.Single(caseFile.Distinctions));
    }

    [Fact]
    public void ConductDoesNotStoreRejectedCases()
    {
        CheckrOf<Case> check =
            from reject in Checkr.RejectCaseWhen(() => true)
            from expectation in Checkr.Expect("always fails", () => false)
            select Case.Closed;
        var custodian = new InMemoryCustodian([]);
        var configured = check.Configure(config => config with
        {
            FileAs = "rejected cases",
            Custodian = custodian,
            ShrinkMode = ShrinkMode.None
        });

        configured.Conduct(
            1.Investigations(),
            3.Runs(),
            1.ExecutionsPerRun());

        var inquiry = Assert.IsType<Inquiry>(custodian.Records["rejected cases"]);
        Assert.Equal(3, inquiry.NumberOfRuns);
        Assert.Equal(0, inquiry.DistinctFailureCount);
        Assert.Empty(custodian.Entries);
    }
}
