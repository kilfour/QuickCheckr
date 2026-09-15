using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr.Tests.Verification;

public class DistinctionTests
{
    [Fact]
    public void InlineDistinctionsProduceSeparateFailureCategories()
    {
        var distinctions = new Queue<bool>([false, true]);
        var currentDistinction = false;
        var distinctionCalls = 0;
        CheckrOf<Case> check = async state =>
        {
            if (!state.EvidenceRecorder.IsReplay)
                currentDistinction = distinctions.Dequeue();

            await Checkr.Distinguish(() =>
            {
                distinctionCalls++;
                return currentDistinction;
            })(state).ConfigureAwait(false);
            return await Checkr.Expect("always fails", () => false)(state).ConfigureAwait(false);
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

    [Fact]
    public void DirectiveCanRegroupAndRejectInlineDistinctions()
    {
        var configured = GetConfiguredCheckr([1, 2, 3], out var custodian);
        var directive = new Directive
        {
            Distinguish = caseFile => (int)caseFile.Distinctions.Single()! % 2,
            Reject = caseFile => (int)caseFile.Distinctions.Single()! == 2
        };

        var exception = Assert.Throws<FalsifiableException>(() =>
            configured.Conduct(
                1.Investigations(),
                3.Runs(),
                1.ExecutionsPerRun(),
                directive));

        var inquiry = Assert.IsType<Inquiry>(exception.TheCaseFile);
        Assert.Equal(3, inquiry.NumberOfRuns);
        Assert.Equal(1, inquiry.DistinctFailureCount);
        Assert.Single(custodian.Entries);
    }

    [Fact]
    public void DirectiveStopsStoringCasesAtItsLimit()
    {
        var configured = GetConfiguredCheckr([1, 2, 3], out var custodian);

        var exception = Assert.Throws<FalsifiableException>(() =>
            configured.Conduct(
                1.Investigations(),
                3.Runs(),
                1.ExecutionsPerRun(),
                new Directive { MaxCaseFiles = 2 }));

        var inquiry = Assert.IsType<Inquiry>(exception.TheCaseFile);
        Assert.True(inquiry.MaxStoredCaseFilesReached);
        Assert.Equal(2, inquiry.DistinctFailureCount);
        Assert.Equal(2, custodian.Entries.Count);
    }

    [Fact]
    public void DirectiveDoesNotTreatResolvedVaultEntriesAsDistinctFailures()
    {
        CheckrOf<Case> check =
            from expectation in Checkr.Expect("always passes", () => true)
            select Case.Closed;
        var custodian = new InMemoryCustodian([new VaultEntry(123, 1)]);
        var configured = check.Configure(config => config with
        {
            FileAs = "resolved distinction",
            Custodian = custodian,
            ShrinkMode = ShrinkMode.None
        });

        configured.Conduct(
            1.Investigations(),
            1.Runs(),
            1.ExecutionsPerRun(),
            new Directive());

        var inquiry = Assert.IsType<Inquiry>(custodian.Records["resolved distinction"]);
        Assert.Equal(0, inquiry.DistinctFailureCount);
    }

    private static ConfiguredCheckr GetConfiguredCheckr(
        IEnumerable<int> distinctions,
        out InMemoryCustodian custodian)
    {
        var remaining = new Queue<int>(distinctions);
        var currentDistinction = 0;
        CheckrOf<Case> check = async state =>
        {
            if (!state.EvidenceRecorder.IsReplay)
                currentDistinction = remaining.Dequeue();

            await Checkr.Distinguish(() => currentDistinction)(state).ConfigureAwait(false);
            return await Checkr.Expect("always fails", () => false)(state).ConfigureAwait(false);
        };
        var configuredCustodian = new InMemoryCustodian([]);
        custodian = configuredCustodian;
        return check.Configure(config => config with
        {
            FileAs = "directive distinctions",
            Custodian = configuredCustodian,
            ShrinkMode = ShrinkMode.None
        });
    }
}
