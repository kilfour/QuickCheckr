namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

internal sealed record Joiner
{
    private bool isFirst = true;
    public Joiner Start() => this with { isFirst = true };
    public bool NeedsSeparator()
    {
        var result = isFirst;
        isFirst = false;
        return !result;
    }
}
