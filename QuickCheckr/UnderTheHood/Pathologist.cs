namespace QuickCheckr.UnderTheHood;

public class Pathologist(AutopsyProbe microtome)
{
    private readonly AutopsyProbe microtome = microtome;
    public bool IsWatching { get; } = !microtome.IsEmpty;

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
        microtome.Observe([.. path]);
        return new PathologyScope(this);
    }

    public void Pop()
    {
        if (path.Count == 0)
            throw new InvalidOperationException("Cannot pop an empty pathology path.");
        path.RemoveAt(path.Count - 1);
    }
}

