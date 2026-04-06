using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction;

public static class Select
{
    public static BalancedCharSelectorBuilder Balanced(char open, char close) => new(open, close, null);

    public static ISelector While(Func<char, bool> predicate)
        => new WhileSelector(predicate);

    public static ISelector OneOf(params char[] options)
        => new SkipWhile(a => !options.Contains(a)).OneOf(options);
}
