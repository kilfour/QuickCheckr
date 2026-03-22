using System.Numerics;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.ReduceTowardsT;

public class ReduceTowards<T>(T target)
    where T : struct, IBinaryInteger<T>, ISignedNumber<T>
{
    private readonly T target = target;
    private bool initialized;
    private T left = T.Zero;
    private T right = T.Zero;
    private T probe = T.Zero;
    private bool targetIsOnRight;
    private bool witnessedPass;

    public Reduction Step(ReductionResult result)
    {
        var current = result.ValueAs<T>();

        if (!initialized)
        {
            initialized = true;

            if (current == target)
                return new Reduction.Unbounded(current);

            ConfigureBounds(current, target);
            probe = FirstProbe(left, right, current, target);
            return new Reduction.Next(probe);
        }

        return StepTowardsTarget(result.Failed);
    }

    private Reduction StepTowardsTarget(bool failed)
    {
        if (!failed)
        {
            witnessedPass = true;
            if (targetIsOnRight)
                right = probe;
            else
                left = probe;
        }
        else
        {
            if (targetIsOnRight)
                left = probe;
            else
                right = probe;
        }

        if (Adjacent(left, right))
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

        probe = Mid(left, right);
        return new Reduction.Next(probe);
    }

    private T FailingEdge() => targetIsOnRight ? left : right;

    private void ConfigureBounds(T a, T b)
    {
        targetIsOnRight = a < b;
        if (targetIsOnRight)
        {
            left = a;
            right = b;
        }
        else
        {
            left = b;
            right = a;
        }
    }

    private static T FirstProbe(T left, T right, T start, T target)
    {
        var mid = Mid(left, right);
        if (mid == start || mid == target)
            return target;
        return mid;
    }

    private static bool Adjacent(T left, T right) => right - left <= T.One;

    private static T Mid(T a, T b)
        => a + ((b - a) / T.CreateChecked(2));
}
