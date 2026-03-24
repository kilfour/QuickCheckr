using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<short>> ShortTowardsZero
        => () => new ReduceShort().TowardsZero;

    private class ReduceShort
    {
        private short left;
        private short right;
        private short probe;
        private bool initialized;
        private bool positive;

        public Reduction TowardsZero(ReductionResult result)
        {
            var currentFailing = result.ValueAs<short>();

            if (!initialized)
            {
                initialized = true;

                if (currentFailing == 0)
                    return new Reduction.Stop(currentFailing);

                positive = currentFailing > 0;

                if (positive)
                {
                    left = 0;              // known passing side
                    right = currentFailing; // known failing side
                }
                else
                {
                    left = currentFailing; // known failing side
                    right = 0;             // known passing side
                }

                probe = Mid(left, right);
                return probe == currentFailing
                    ? new Reduction.Stop(currentFailing)
                    : new Reduction.Next(probe);
            }

            if (positive)
            {
                if (result.Failed)
                    right = probe; // probe failed, so it is the new failing boundary
                else
                    left = probe;  // probe passed, so it is the new passing boundary

                if (right - left <= 1)
                    return new Reduction.Stop(right);
            }
            else
            {
                if (result.Failed)
                    left = probe;  // probe failed, so it is the new failing boundary
                else
                    right = probe; // probe passed, so it is the new passing boundary

                if (right - left <= 1)
                    return new Reduction.Stop(left);
            }

            probe = Mid(left, right);

            var currentBest = result.ValueAs<short>();
            return probe == currentBest
                ? new Reduction.Stop(currentBest)
                : new Reduction.Next(probe);
        }

        private static short Mid(short a, short b)
            => (short)(a + ((b - a) / 2));
    }
}