using QuickCheckr.UnderTheHood;
namespace QuickCheckr;
public static partial class Checkr
{
    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<Task<bool>> condition)
        => Evaluate.ExpectAsync(key, _ => condition(), true, null, _ => Task.FromResult(predicate()));

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<Task<bool>> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, _ => condition(), true, () => [message()], _ => Task.FromResult(predicate()));

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<Task<bool>> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, _ => condition(), true, messages, _ => Task.FromResult(predicate()));

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<CancellationToken, Task<bool>> condition)
        => Evaluate.ExpectAsync(key, condition, true, null, _ => Task.FromResult(predicate()));

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<CancellationToken, Task<bool>> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, condition, true, () => [message()], _ => Task.FromResult(predicate()));

    public static CheckrOf<Case> ExpectWhen(string key, Func<bool> predicate, Func<CancellationToken, Task<bool>> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, condition, true, messages, _ => Task.FromResult(predicate()));

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<bool> condition)
        => Evaluate.ExpectAsync(key, _ => Task.FromResult(condition()), true, null, _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<bool> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, _ => Task.FromResult(condition()), true, () => [message()], _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<bool> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, _ => Task.FromResult(condition()), true, messages, _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<Task<bool>> condition)
        => Evaluate.ExpectAsync(key, _ => condition(), true, null, _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<Task<bool>> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, _ => condition(), true, () => [message()], _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<Task<bool>> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, _ => condition(), true, messages, _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<CancellationToken, Task<bool>> condition)
        => Evaluate.ExpectAsync(key, condition, true, null, _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<CancellationToken, Task<bool>> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, condition, true, () => [message()], _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<Task<bool>> predicate, Func<CancellationToken, Task<bool>> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, condition, true, messages, _ => predicate());

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<bool> condition)
        => Evaluate.ExpectAsync(key, _ => Task.FromResult(condition()), true, null, predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<bool> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, _ => Task.FromResult(condition()), true, () => [message()], predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<bool> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, _ => Task.FromResult(condition()), true, messages, predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<Task<bool>> condition)
        => Evaluate.ExpectAsync(key, _ => condition(), true, null, predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<Task<bool>> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, _ => condition(), true, () => [message()], predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<Task<bool>> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, _ => condition(), true, messages, predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<CancellationToken, Task<bool>> condition)
        => Evaluate.ExpectAsync(key, condition, true, null, predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<CancellationToken, Task<bool>> condition, Func<string> message)
        => Evaluate.ExpectAsync(key, condition, true, () => [message()], predicate);

    public static CheckrOf<Case> ExpectWhen(string key, Func<CancellationToken, Task<bool>> predicate, Func<CancellationToken, Task<bool>> condition, Func<List<string>>? messages)
        => Evaluate.ExpectAsync(key, condition, true, messages, predicate);

}
