using QuickCheckr.UnderTheHood;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr.Tests.Verification;

public class ConductAsyncTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AwaitsSuspendingCallbacksDuringInvestigationAndVaultReplay(bool replay)
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completed = false;
        var custodian = new InMemoryCustodian(replay ? [new VaultEntry(123, 1)] : []);
        var configured = Checkr.Expect("suspends", async () =>
        {
            entered.TrySetResult();
            await release.Task;
            completed = true;
            return true;
        }).Configure(config => config with { FileAs = "async", Custodian = custodian, ShrinkMode = ShrinkMode.None });

        var conduct = configured.ConductAsync((replay ? 0 : 1).Investigations(), 1.Runs(), 1.ExecutionsPerRun(), TestContext.Current.CancellationToken);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.False(conduct.IsCompleted);
            Assert.False(completed);
        }
        finally
        {
            release.TrySetResult();
        }
        Assert.Same(configured, await conduct.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.True(completed);
        Assert.True(custodian.Records.ContainsKey("async"));
    }

    [Fact]
    public async Task PreCancelledCallDoesNotExecuteOrFileRecords()
    {
        var calls = 0;
        var custodian = new InMemoryCustodian([]);
        var configured = Checkr.Expect("passes", () => { calls++; return true; })
            .Configure(config => config with { FileAs = "async", Custodian = custodian });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => configured.ConductAsync(
            1.Investigations(), 1.Runs(), 1.ExecutionsPerRun(), cancellation.Token));
        Assert.Equal(0, calls);
        Assert.Empty(custodian.Records);
    }

    [Fact]
    public async Task ReportsAndStoresDistinctFailuresUsingDirective()
    {
        var custodian = new InMemoryCustodian([]);
        var configured = Checkr.Expect("fails", async () => { await Task.Yield(); return false; })
            .Configure(config => config with { FileAs = "async", Custodian = custodian, ShrinkMode = ShrinkMode.None });

        var exception = await Assert.ThrowsAsync<FalsifiableException>(() => configured.ConductAsync(
            2.Investigations(), 1.Runs(), 1.ExecutionsPerRun(), new Directive { MaxCaseFiles = 1 }, TestContext.Current.CancellationToken));
        var inquiry = Assert.IsType<Inquiry>(exception.TheCaseFile);
        Assert.Equal(1, inquiry.DistinctFailureCount);
        Assert.True(inquiry.MaxStoredCaseFilesReached);
        Assert.Single(custodian.Entries);
    }
}


