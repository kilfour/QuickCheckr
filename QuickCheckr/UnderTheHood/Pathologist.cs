using QuickCheckr.Diagnostics;

namespace QuickCheckr.UnderTheHood;

public class Pathologist(AutopsyProbe autopsyProbe, CheckrState state)
{
    public bool IsWatching { get; } = !autopsyProbe.IsEmpty;

    private readonly List<string> path = [];

    public void Inform(params object[] paths)
    {
        using var _ = Examine(paths);
    }

    public PathologyScope Examine(params object[] paths)
    {
        if (!IsWatching)
            return default;
        var newPath = string.Join('-', paths);
        path.Add(newPath);
        autopsyProbe.Observe(state, [.. path]);
        return new PathologyScope(this);
    }

    public void Pop()
    {
        if (path.Count == 0)
            throw new InvalidOperationException("Cannot pop an empty pathology path.");
        path.RemoveAt(path.Count - 1);
    }
}

