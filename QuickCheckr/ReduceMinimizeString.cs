using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<ReductionResult, Reduction> MinimizeString
        => new MinimizeStringFactory().Reduce;

    private class MinimizeStringFactory
    {
        private int index = 0;
        public Reduction Reduce(ReductionResult result)
        {
            var str = result.ValueAs<string>();
            if (result.Passed)
                index++;
            if (index >= str.Length)
                return Reduction.Stop(str);
            return Reduction.Next(str.Remove(index, 1));
        }
    }
}