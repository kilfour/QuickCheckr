namespace QuickCheckr.Tests.Tools.ThePress;

public abstract class AbstractArticle<T>(T deposition)
{
    protected readonly T deposition = deposition;

    public T Read() { return deposition; }
}
