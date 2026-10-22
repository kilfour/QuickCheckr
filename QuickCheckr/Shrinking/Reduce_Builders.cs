using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Shrinking;

public static partial class Reduce
{
    private class StopOnFirstPassedBuilder<T>(Func<T, IEnumerable<T>> factory)
    {
        private IEnumerator<T>? enumerator = null;
        private bool exhausted = false;
        private T? lastFailure = default;

        public Reduction Reduce(ReductionResult reductionResult)
        {
            if (enumerator is null)
            {
                enumerator = factory(reductionResult.ValueAs<T>()).GetEnumerator();
                exhausted = !enumerator.MoveNext();
            }
            if (reductionResult.Failed)
                lastFailure = reductionResult.ValueAs<T>();
            else
                return new Reduction.Stop(lastFailure!);
            if (exhausted)
                return new Reduction.Unbounded(lastFailure!);
            var next = enumerator.Current;
            exhausted = !enumerator.MoveNext();
            return new Reduction.Next(next!);
        }
    }

    private class ReportLastFailureBuilder<T>(Func<T, IEnumerable<T>> factory)
    {
        private IEnumerator<T>? enumerator = null;
        private bool exhausted = false;
        private T? lastFailure = default;
        private Maybe<object> witness = Maybe<object>.Nothing();


        public Reduction Reduce(ReductionResult reductionResult)
        {
            if (enumerator is null)
            {
                enumerator = factory(reductionResult.ValueAs<T>()).GetEnumerator();
                exhausted = !enumerator.MoveNext();
            }
            if (reductionResult.Failed)
                lastFailure = reductionResult.ValueAs<T>();
            else
                witness = Maybe<object>.Just(reductionResult.Value);
            if (exhausted)
            {
                if (witness.HasValue)
                    return new Reduction.Stop(lastFailure!);
                return new Reduction.Unbounded(lastFailure!);
            }

            var next = enumerator.Current;
            exhausted = !enumerator.MoveNext();
            return new Reduction.Next(next!);
        }
    }
}
