using System.Diagnostics;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Experimental.C_CheckingTheClock.Chrono;

public record TimedAction<T>(TimeSpan Duration, T Value);

public class Timr
{
    public async static Task<TimedAction<T>> For<T>(Func<Task<T>> action)
    {
        var start = Stopwatch.GetTimestamp();
        var result = await action().ConfigureAwait(false);
        TimeSpan elapsed = Stopwatch.GetElapsedTime(start);
        return new TimedAction<T>(elapsed, result);
    }

    public static CheckrOf<Case> Expect<T>(string key, Func<TimrExpect<T>, ExpectationResult> evaluation)
        => Evaluate.Expectation(key, _ => evaluation(new TimrExpect<T>()), true);
}

public class TimrExpect<T>()
{
    public TimrCompare<T> First(TimedAction<T> first) => new(first);
}

public class TimrCompare<T>(TimedAction<T> first)
{
    public TimerCompareFasterThan<T> IsFasterThan(TimedAction<T> second) => new(first, second);
}

public class TimerCompareFasterThan<T>(TimedAction<T> first, TimedAction<T> second)
{
    public ExpectationResult ByAtLeast(int milliseconds)
        => second.Duration - first.Duration >= TimeSpan.FromMilliseconds(milliseconds)
                ? ExpectationResult.Passed()
                : ExpectationResult.Failed(
                    [
                        $"Delta:       {milliseconds} ms",
                        $"First took:  {first.Duration}",
                        $"Second took: {second.Duration}",
                        "-----------------------------",
                        $"Missed by:   {first.Duration - second.Duration + TimeSpan.FromMilliseconds(milliseconds)}"
                    ]);
}