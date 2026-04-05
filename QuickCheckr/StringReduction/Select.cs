using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction;

public static class Select
{
    public static BalancedCharSelectorBuilder Balanced(char open, char close) => new(open, close);

    public static ISelector While(Func<char, bool> predicate)
        => new WhileSelector(predicate);

    public static ISelector OneOf(params char[] options)
        => new SkipWhile(a => !options.Contains(a)).OneOf(options);
    //=> new WhileSelector(a => !options.Contains(a), null).OneOf(options);


    public class BalancedCharSelectorBuilder(char open, char close)
    {
        public ISelector Inside() => new InsideCharSelector(open, close);
        public ISelector Enclosed() => new EnclosedCharSelector(open, close);
        public ISelector Delimiters() => new DelimiterCharSelector(open, close);
    }
}


