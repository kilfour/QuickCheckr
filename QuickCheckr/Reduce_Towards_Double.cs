using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<double>> Towards(double target, int precision = 15)
    {
        if (double.IsNaN(target))
            throw new ArgumentOutOfRangeException(nameof(target), target, "Target cannot be NaN.");
        if (precision is < 0 or > 15)
            throw new ArgumentOutOfRangeException(nameof(precision), precision, "Precision must be between 0 and 15.");

        return () => new ReduceDoubleTowards(target, precision).Step;
    }

    private sealed class ReduceDoubleTowards(double target, int precision)
    {
        private const ulong SignMask = 0x8000000000000000;

        private double left;
        private double right;
        private double probe;
        private bool initialized;
        private bool targetIsOnRight;
        private bool witnessedPass;

        public Reduction Step(ReductionResult result)
        {
            var current = result.ValueAs<double>();

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

        private void ConfigureBounds(double current)
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

        private bool TryGetNextProbe(out double next)
        {
            next = Math.Round(Mid(left, right), precision);
            if (next > left && next < right)
                return true;

            next = Math.Round(ArithmeticMid(left, right), precision);
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

        private double FailingEdge() => targetIsOnRight ? left : right;

        private static double Mid(double left, double right)
        {
            var leftBits = ToOrderedBits(left);
            var rightBits = ToOrderedBits(right);
            var midpointBits = leftBits + ((rightBits - leftBits) / 2);
            return FromOrderedBits(midpointBits);
        }

        private static double ArithmeticMid(double left, double right)
        {
            if (left < 0d && right > 0d)
                return (left / 2d) + (right / 2d);

            return left + ((right - left) / 2d);
        }

        private static ulong ToOrderedBits(double value)
        {
            var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
            return (bits & SignMask) == 0 ? bits | SignMask : ~bits;
        }

        private static double FromOrderedBits(ulong value)
        {
            var bits = (value & SignMask) == 0 ? ~value : value & ~SignMask;
            return BitConverter.Int64BitsToDouble(unchecked((long)bits));
        }
    }
}
