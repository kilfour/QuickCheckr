using System.Numerics;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.ReduceTowardsT;

public class ReduceTowards<T>(T target)
    where T : struct, IBinaryInteger<T>, ISignedNumber<T>
{
    private readonly T target = target;
    private T left = target;
    private T right = target;
    private T probe = target;
    private bool initialized;
    private bool targetIsOnLeft;
    private bool hasPassed;

    public Reduction Step(ReductionResult result)
    {
        var currentFailing = result.ValueAs<T>();

        if (!initialized)
        {
            initialized = true;

            if (currentFailing == target)
                return new Reduction.Stop(currentFailing);

            targetIsOnLeft = target < currentFailing;

            if (targetIsOnLeft)
            {
                left = target;
                right = currentFailing;
            }
            else
            {
                left = currentFailing;
                right = target;
            }

            probe = Mid(left, right);
            return probe == currentFailing
                ? Stop(currentFailing)
                : new Reduction.Next(probe);
        }

        if (targetIsOnLeft)
        {
            if (result.Failed)
            {
                right = probe;
            }
            else
            {
                left = probe;
                hasPassed = true;
            }

            if (right - left < T.One)
                return Stop(right);
        }
        else
        {
            if (result.Failed)
            {
                left = probe;
            }
            else
            {
                right = probe;
                hasPassed = true;
            }

            if (right - left < T.One)
                return Stop(left);
        }

        if (probe == target)
            return Stop(target);

        probe = Mid(left, right);

        return probe == currentFailing
            ? Stop(currentFailing)
            : new Reduction.Next(probe);
    }

    private Reduction Stop(T value) =>
        hasPassed ? new Reduction.Stop(value) : new Reduction.Unbounded(value);

    private static T Mid(T a, T b)
        => a + ((b - a) / T.CreateChecked(2));
}
