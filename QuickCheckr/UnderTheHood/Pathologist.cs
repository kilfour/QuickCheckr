using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood;

public class Pathologist
{
    public static Pathologist From(List<string> focuses) =>
        focuses.Count > 0 ? new() { IsWatching = true, focuses = focuses } : new();

    public bool IsWatching { get; private set; }
    private List<string> focuses = [];
    private readonly FileLogArtery artery = FileLog.Write("pathology-results.log");
    private readonly Stack<string> path = new();

    public void Inform(params object[] paths)
    {
        using var _ = Examine(paths);
    }

    public PathologyScope Examine(params object[] paths)
    {
        if (!IsWatching)
            return default;
        var newPath = string.Join('-', paths);
        path.Push(newPath);
        var source = string.Join('-', path.Reverse());
        if (focuses.Any(a => source.StartsWith(a)))
            artery.Absorb(source);
        return new PathologyScope(this);
    }

    public void Pop()
    {
        path.Pop();
    }

    private Pathologist() { }
}

public readonly struct PathologyScope : IDisposable
{
    private readonly Pathologist? pathologist;

    public PathologyScope(Pathologist? pathologist)
        => this.pathologist = pathologist;

    public void Dispose()
        => pathologist?.Pop();
}



