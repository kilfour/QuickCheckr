namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public record Joiner
{
    private bool isFirst = true;
    public Joiner Prime() => this with { isFirst = true };
    public bool NeedsSeparator()
    {
        var result = isFirst;
        isFirst = false;
        return !result;
    }
}
