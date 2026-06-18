using QuickPulse.Bolts;

namespace QuickCheckr.Tests.Investigations.DisposableStash;

public class TryingItOut
{
    [Fact]
    public void Checking()
    {
        var disposed = new Cell<bool>(false);
        var checkr =
            from _ in Trackr.Stashed(() => new DisposableStash(disposed))
            select Case.Closed;
        checkr.Run();
        Assert.True(disposed.Value);
    }

    public class DisposableStash(Cell<bool> flag) : IDisposable
    {
        public void Dispose()
        {
            flag.Value = true;
        }
    }
}