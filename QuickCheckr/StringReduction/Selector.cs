using QuickCheckr.StringReduction.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.StringReduction;

public interface ISelector
{
    ISelection[] Extract(string input);
    Func<Reducer<string>> Remove();
}

public static class Select
{
    public static BalancedCharSelectorBuilder Balanced(char open, char close) => new(open, close);

    public class BalancedCharSelectorBuilder(char open, char close)
    {
        public ISelector Inside() => new InsideCharSelector(open, close);
        public ISelector Enclosed() => new EnclosedCharSelector(open, close);
        public ISelector Delimiters() => new DelimiterCharSelector(open, close);
    }
}

