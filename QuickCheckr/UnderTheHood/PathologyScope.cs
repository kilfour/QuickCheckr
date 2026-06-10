namespace QuickCheckr.UnderTheHood;

public readonly struct PathologyScope(Pathologist? pathologist) : IDisposable
{
    private readonly Pathologist? pathologist = pathologist;

    public void Dispose()
        => pathologist?.Pop();
}

