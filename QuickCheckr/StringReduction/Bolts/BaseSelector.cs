using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.StringReduction.Bolts;

public abstract class BaseSelector : IBaseSelector
{
    private ISelector AsISelector() => (this as ISelector)!;
    public ISelector Or(ISelector other) => new OrSelector(AsISelector(), other);
    public ISelector While(Func<char, bool> predicate) => new WhileSelector(predicate, AsISelector());
    public ISelector OneOf(params char[] options) => new OneOfSelector(AsISelector(), options);

    public Func<Reducer<string>> Remove() => () => new Factory(
                AsISelector(),
                (selection, current) => selection.Remove(current))
            .Reduce;


    public Func<Reducer<string>> Replace(string replacement) =>
        () => new Factory(
                AsISelector(),
                (selection, current) => selection.Replace(current, replacement))
            .Reduce;

    protected class Factory(ISelector selector, Func<ISelection, string, string> selectorFunction)
    {
        private bool witnessed = false;
        private bool initialized = false;
        private string current = "";
        private string lastFailure = "";
        public Reduction Reduce(ReductionResult result)
        {
            var str = result.ValueAs<string>();
            if (!initialized)
            {
                initialized = true;
                current = str;
                lastFailure = str;
            }
            if (result.Passed)
                witnessed = true;
            else
            {
                current = str;
                lastFailure = str;
            }
            var selection = selector.Extract(current);
            if (selection == null)
            {
                if (witnessed)
                    return new Reduction.Stop(lastFailure);
                return new Reduction.Unbounded(lastFailure);
            }
            current = selectorFunction(selection, current);
            return new Reduction.Next(current);
        }
    }
}
