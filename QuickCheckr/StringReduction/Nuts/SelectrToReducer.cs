using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;


namespace QuickCheckr.StringReduction.Nuts;

public static class SelectrToReducer
{
    public static Func<Reducer<string>> Reduce(this SelectrOf<Selection> selector) =>
        () => new Factory(selector).Reduce;

    private class Factory(SelectrOf<Selection> selectr)
    {
        private SelectrRun? run = null;
        private bool witnessed = false;
        private bool initialized = false;
        private string lastFailure = "";
        public Reduction Reduce(ReductionResult result)
        {
            var str = result.ValueAs<string>();
            if (!initialized)
            {
                initialized = true;
                lastFailure = str;
                run = selectr.Run(str);
            }
            if (result.Passed)
                witnessed = true;
            else
            {
                lastFailure = str;
            }

            if (run!.EndOfText)
            {
                if (witnessed)
                    return new Reduction.Stop(lastFailure);
                return new Reduction.Unbounded(lastFailure);
            }

            return new Reduction.Next(run!.Next());
        }
    }
}
