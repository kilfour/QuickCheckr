using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<decimal>> Towards(decimal target, int precision = 28)
    {
        if (precision is < 0 or > 28)
            throw new ArgumentOutOfRangeException(nameof(precision), precision, "Precision must be between 0 and 28.");

        return () => new ReduceDecimalTowards(target, precision).Step;
    }

    private sealed class ReduceDecimalTowards(decimal target, int precision)
    {
        private decimal left;
        private decimal right;
        private decimal probe;
        private bool initialized;
        private bool targetIsOnRight;
        private bool witnessedPass;

        public Reduction Step(ReductionResult result)
        {
            var current = result.ValueAs<decimal>();

            if (!initialized)
            {
                initialized = true;

                if (current == target)
                    return new Reduction.Unbounded(current);

                ConfigureBounds(current);
                probe = TryGetNextProbe(out var firstProbe) ? firstProbe : target;
                return new Reduction.Next(probe);
            }

            UpdateBounds(result.Failed);

            if (!TryGetNextProbe(out var next))
            {
                if (!witnessedPass && FailingEdge() != target)
                {
                    probe = target;
                    return new Reduction.Next(probe);
                }

                return witnessedPass
                    ? new Reduction.Stop(FailingEdge())
                    : new Reduction.Unbounded(FailingEdge());
            }

            probe = next;
            return new Reduction.Next(probe);
        }

        private void ConfigureBounds(decimal current)
        {
            targetIsOnRight = current < target;
            if (targetIsOnRight)
            {
                left = current;
                right = target;
            }
            else
            {
                left = target;
                right = current;
            }
        }

        private bool TryGetNextProbe(out decimal next)
        {
            next = decimal.Round(Mid(left, right), precision);
            return next > left && next < right;
        }

        private void UpdateBounds(bool failed)
        {
            if (!failed)
            {
                witnessedPass = true;
                if (targetIsOnRight)
                    right = probe;
                else
                    left = probe;
                return;
            }

            if (targetIsOnRight)
                left = probe;
            else
                right = probe;
        }

        private decimal FailingEdge() => targetIsOnRight ? left : right;

        private static decimal Mid(decimal left, decimal right)
        {
            // Subtraction can overflow when the bounds have opposite signs.
            if (left < 0m && right > 0m)
                return (left / 2m) + (right / 2m);

            return left + ((right - left) / 2m);
        }
    }
}
