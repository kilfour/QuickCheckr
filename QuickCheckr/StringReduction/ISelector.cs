using QuickCheckr.StringReduction.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.StringReduction;

public interface IBaseSelector
{
    ISelector Or(ISelector selector);
    ISelector While(Func<char, bool> predicate);
    ISelector OneOf(params char[] stopOn);
    BalancedCharSelectorBuilder Balanced(char open, char close);

}

public interface ISelector : IBaseSelector
{
    ISelection? Extract(string text);
    Func<Reducer<string>> Remove();
    Func<Reducer<string>> Replace(string replacement);
}
