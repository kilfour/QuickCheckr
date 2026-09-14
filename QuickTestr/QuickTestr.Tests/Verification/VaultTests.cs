using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickTestr.Bolts;

namespace QuickTestr.Tests.Verification;

public class VaultTests
{
    [Fact]
    public void FillVaultClassifiesEveryFailingRunInline()
    {
        var classificationCalls = 0;
        var journalist = new Journalist();
        var runner = Testr.Named("Classified vault")
            .For(Fuzzr.Constant(42))
            .Assert(_ => false)
            .StoreCaseFiles(journalist);
        var policy = new VaultPolicy<int>(input =>
        {
            classificationCalls++;
            return input;
        });

        var exception = Assert.Throws<FalsifiableException>(() =>
            runner.FillVault(1.Searches(), 3.Runs(), policy));

        var inquiry = Assert.IsType<Inquiry>(exception.TheCaseFile);
        Assert.Equal(3, classificationCalls);
        Assert.Equal(3, inquiry.NumberOfRuns);
        Assert.Equal(1, inquiry.DistinctFailureCount);
        Assert.Single(journalist.Entries);
    }

    [Fact]
    public void FillVaultSkipsInputsRejectedByThePolicy()
    {
        var skipCalls = 0;
        var journalist = new Journalist();
        var runner = Testr.Named("Filtered vault")
            .For(Fuzzr.Constant(42))
            .Assert(_ => false)
            .StoreCaseFiles(journalist);
        var policy = new VaultPolicy<int>(
            input => input,
            SkipWhen: _ =>
            {
                skipCalls++;
                return true;
            });

        runner.FillVault(1.Searches(), 3.Runs(), policy);

        var inquiry = Assert.IsType<Inquiry>(journalist.GetRecord());
        Assert.Equal(3, skipCalls);
        Assert.Equal(0, inquiry.DistinctFailureCount);
        Assert.Empty(journalist.Entries);
    }

    [Fact]
    public void FillVaultHonorsThePolicyLimit()
    {
        var nextCategory = 0;
        var journalist = new Journalist();
        var runner = Testr.Named("Limited vault")
            .For(Fuzzr.Constant(42))
            .Assert(_ => false)
            .StoreCaseFiles(journalist);
        var policy = new VaultPolicy<int>(
            _ => ++nextCategory,
            MaxStoredFailures: 2);

        var exception = Assert.Throws<FalsifiableException>(() =>
            runner.FillVault(1.Searches(), 3.Runs(), policy));

        var inquiry = Assert.IsType<Inquiry>(exception.TheCaseFile);
        Assert.True(inquiry.MaxStoredCaseFilesReached);
        Assert.Equal(2, inquiry.DistinctFailureCount);
        Assert.Equal(2, journalist.Entries.Count);
    }

    [Fact]
    public void TwoInputOracleUsesTheTupleForItsVaultPolicy()
    {
        var classifiedInput = default((int, int));
        var journalist = new Journalist();
        var runner = Testr.Named("Two-input oracle vault")
            .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
            .Expected((left, right) => left + right)
            .Actual((left, right) => left + right + 1)
            .StoreCaseFiles(journalist);
        var policy = new VaultPolicy<(int, int)>(input =>
        {
            classifiedInput = input;
            return input;
        });

        Assert.Throws<FalsifiableException>(() =>
            runner.FillVault(1.Searches(), 1.Runs(), policy));

        Assert.Equal((20, 22), classifiedInput);
        Assert.Single(journalist.Entries);
    }
}
