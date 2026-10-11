namespace QuickCheckr.Tests.Concurrency;

public class AsyncStashedTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task AsyncFactoriesStoreTheValueOncePerRun(bool keyed, bool withToken)
    {
        using var cancellation = new CancellationTokenSource();
        var calls = 0;
        var resource = new Resource();
        async Task<Resource> Create(CancellationToken token)
        {
            Assert.Equal(cancellation.Token, token);
            await Task.Yield();
            calls++;
            return resource;
        }
        var stash = (keyed, withToken) switch
        {
            (false, false) => Trackr.Stashed(() => Create(cancellation.Token)),
            (true, false) => Trackr.Stashed("resource", () => Create(cancellation.Token)),
            (false, true) => Trackr.Stashed<Resource>(Create),
            _ => Trackr.Stashed<Resource>("resource", Create)
        };
        var retrieve = keyed ? Trackr.GetStashed<Resource>("resource") : Trackr.GetStashed<Resource>();
        var check =
            from value in stash
            from stored in retrieve
            from verify in Checkr.Act("verify", () => Assert.Same(value, stored))
            select Case.Closed;

        await check.RunAsync(3.ExecutionsPerRun(), cancellation.Token);

        Assert.Equal(1, calls);
        Assert.Equal(1, resource.Disposals);
    }

    [Fact]
    public async Task CancellationAfterCreationStillDisposesTheStoredValue()
    {
        using var cancellation = new CancellationTokenSource();
        var resource = new Resource();
        var check = Trackr.Stashed(async token =>
        {
            Assert.Equal(cancellation.Token, token);
            await Task.Yield();
            cancellation.Cancel();
            return resource;
        }).Select(_ => Case.Closed);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => check.RunAsync(cancellation.Token));
        Assert.Equal(1, resource.Disposals);
    }

    private sealed class Resource : IAsyncDisposable
    {
        public int Disposals { get; private set; }
        public ValueTask DisposeAsync()
        {
            Disposals++;
            return ValueTask.CompletedTask;
        }
    }
}
