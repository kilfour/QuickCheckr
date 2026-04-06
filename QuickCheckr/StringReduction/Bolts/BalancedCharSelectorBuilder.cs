namespace QuickCheckr.StringReduction.Bolts;

public class BalancedCharSelectorBuilder(char open, char close, ISelector? left)
{
    public ISelector Inside() => new InsideCharSelector(open, close, left);
    public ISelector Enclosed() => new EnclosedCharSelector(open, close, left);
    public ISelector Delimiters() => new DelimiterCharSelector(open, close, left);
}