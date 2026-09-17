using QuickCheckr.UnderTheHood;
using QuickPulse.Show.Bolts;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>Awaits an expectation only when it is active in the current run or replay.</summary>
    public static CheckrOf<Case> Expect(string key, Func<Task<bool>> condition)
        => Expect(key, _ => condition());

    public static CheckrOf<Case> Expect(string key, Func<Task<bool>> condition, Func<string> message)
        => Expect(key, _ => condition(), message);

    public static CheckrOf<Case> Expect(string key, Func<Task<bool>> condition, Func<List<string>>? messages)
        => Expect(key, _ => condition(), messages);

    public static CheckrOf<Case> Expect(string key, Func<Task<bool>> condition,
        Func<ManOfWealthAndTaste, List<string>> messages)
        => Expect(key, _ => condition(), messages);

    /// <summary>Awaits an expectation with the run's cancellation token.</summary>
    public static CheckrOf<Case> Expect(string key, Func<CancellationToken, Task<bool>> condition)
        => Evaluate.ExpectAsync(key, condition, true);

    public static CheckrOf<Case> Expect(string key, Func<CancellationToken, Task<bool>> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, condition, true, () => [message()]);

    public static CheckrOf<Case> Expect(string key, Func<CancellationToken, Task<bool>> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, condition, true, messages);

    public static CheckrOf<Case> Expect(string key, Func<CancellationToken, Task<bool>> condition,
        Func<ManOfWealthAndTaste, List<string>> messages)
        => state => Evaluate.ExpectAsync(key, condition, true, () => messages(state.Editor))(state);
}
