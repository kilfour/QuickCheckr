namespace QuickCheckr.Diagnostics.Bolts;

public abstract record AutopsyPathBuilder(IReadOnlyList<string> Parts)
{
    protected IReadOnlyList<string> Add(string part)
        => [.. Parts, part];
}

public record PassPath(IReadOnlyList<string> Parts) : AutopsyPathBuilder(Parts)
{
    public CheckingPath Pass(int number)
        => new(Add(AutopsyPath.Pass(number)));
}

public record CheckingPath(IReadOnlyList<string> Parts) : AutopsyPathBuilder(Parts)
{
    public ExecutingPath Checking(int executionId)
        => new(Add(AutopsyPath.Checking(executionId)));
}

public record ExecutingPath(IReadOnlyList<string> Parts) : AutopsyPathBuilder(Parts)
{
    public FailedAndRemovedPath Executing(int executionId)
        => new(Add(AutopsyPath.Executing(executionId)));
}

public record FailedAndRemovedPath(IReadOnlyList<string> Parts) : AutopsyPathBuilder(Parts)
{
    public FinishedPath FailedAndRemoved
        => new(Add(AutopsyPath.FailedAndRemoved));
}

public record InputCheckingPath(IReadOnlyList<string> Parts) : AutopsyPathBuilder(Parts)
{
    public InputPath Checking(int executionId)
        => new(Add(AutopsyPath.Checking(executionId)));
}

public record InputPath(IReadOnlyList<string> Parts) : AutopsyPathBuilder(Parts)
{
    public FinishedPath Input(string key)
        => new(Add(AutopsyPath.Input(key)));
}

public record FinishedPath(IReadOnlyList<string> Parts) : AutopsyPathBuilder(Parts);


