namespace QuickCheckr.StringReduction.Bolts;

public class OrSelector(ISelector left, ISelector right) : BaseSelector, ISelector
{
    public ISelection? Extract(string text)
    {
        var selection = left.Extract(text);
        if (selection != null) return selection;
        return right.Extract(text);
    }
}
