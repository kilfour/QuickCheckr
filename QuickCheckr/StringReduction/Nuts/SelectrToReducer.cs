using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;


namespace QuickCheckr.StringReduction.Nuts;

public static class SelectrToReducer
{
    public static Func<Reducer<string>> Reduce(this SelectrOf<Selection> selector) =>
        () => new ReductionSession(selector).Step;

    private sealed class ReductionSession(SelectrOf<Selection> selector)
    {
        private SelectrRun? run;
        private bool witnessedPass;
        private string lastFailingValue = string.Empty;

        public Reduction Step(ReductionResult result)
        {
            if (run is null)
            {
                lastFailingValue = result.ValueAs<string>();
                run = selector.Run(lastFailingValue);
            }
            else if (result.Passed)
            {
                witnessedPass = true;
                run.RejectCandidate();
            }
            else
            {
                lastFailingValue = result.ValueAs<string>();
            }

            if (run.EndOfText)
                return Complete();

            return new Reduction.Next(run.Next());
        }

        private Reduction Complete() => witnessedPass
            ? new Reduction.Stop(lastFailingValue)
            : new Reduction.Unbounded(lastFailingValue);
    }
}
