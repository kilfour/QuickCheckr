using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Shrinking;

public static partial class Reduce
{
    public static Func<Reducer<string>> MinimizeString
        => () => new MinimizeStringFactory(0).Reduce;

    public static Func<Reducer<string>> MinimizeStringTo(int minLength)
        => () => new MinimizeStringFactory(minLength).Reduce;

    private class MinimizeStringFactory(int minLength)
    {
        private int index = 0;
        private bool replacing = false;

        public Reduction Reduce(ReductionResult result)
        {
            if (result.Value is null)
                return new Reduction.Stop(result.Value!);

            var str = result.ValueAs<string>();
            if (str.Length <= minLength)
                return new Reduction.Stop(result.Value!);
            if (result.Passed)
            {
                if (replacing)
                {
                    replacing = false;
                    index++;
                }
                else
                {
                    replacing = true;
                    return new Reduction.Next(str[..index] + 'a' + str[(index + 1)..]);
                }
            }

            if (index >= str.Length)
                return new Reduction.Stop(str);
            return new Reduction.Next(str.Remove(index, 1));
        }
    }
}
