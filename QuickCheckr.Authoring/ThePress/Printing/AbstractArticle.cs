namespace QuickCheckr.Authoring.ThePress.Printing;

public abstract class AbstractArticle<T>(T deposition)
{
    protected readonly T deposition = deposition;

    public T Read() { return deposition; }
}
