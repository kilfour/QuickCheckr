using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;


namespace QuickCheckr.StringReduction.Nuts;

public static partial class Selectr
{
    public static Func<Reducer<string>> Reduce(this SelectrOf<Selection> selector) =>
        () => new Factory(selector).Reduce;

    public static Func<Reducer<string>> Reduce<T>(this SelectrOf<T> selector) =>
        () => new Factory(from _ in selector select Selection.Defined).Reduce;

    private class Factory(SelectrOf<Selection> selectr)
    {
        private SelectrRun? run = null;
        private bool witnessed = false;
        private bool initialized = false;
        // private string current = "";
        private string lastFailure = "";
        public Reduction Reduce(ReductionResult result)
        {
            var str = result.ValueAs<string>();
            if (!initialized)
            {
                initialized = true;
                // current = str;
                lastFailure = str;
                run = selectr.Run(str);
            }
            if (result.Passed)
                witnessed = true;
            else
            {
                // current = str;
                lastFailure = str;
            }

            // var selection = selector.Extract(current);
            if (run!.EndOfText)
            {
                if (witnessed)
                    return new Reduction.Stop(lastFailure);
                return new Reduction.Unbounded(lastFailure);
            }
            // current = selectorFunction(selection, current);
            return new Reduction.Next(run!.Next());
        }
    }
}
