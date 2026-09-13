using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Captures whether a carefully executed action threw and, when it did, the resulting exception.
/// </summary>
public class DelayedResult
{
    public bool Threw { get; }
    public Exception? Exception { get; }

    internal DelayedResult()
    {
        Threw = false;
        Exception = null;
    }

    internal DelayedResult(Exception exception)
    {
        Threw = true;
        Exception = exception;
    }

    public bool ThrewAs<TException>() where TException : Exception
        => Threw && Exception is TException;

    public bool ThrewExactly<T>() where T : Exception
        => Exception?.GetType() == typeof(T);

    public bool ThrewAs<T>(out T ex) where T : Exception
    {
        if (Threw && Exception is T matched)
        {
            ex = matched;
            return true;
        }
        ex = default!;
        return false;
    }

    public List<string> DescribeException<TExpected>()
    {
        var expected = typeof(TExpected).Name;
        if (Threw)
        {
            var actual = Exception!.GetType().Name;
            var message = Exception!.Message;
            return [
                $"Expected: {expected}",
                $"Actual:   {actual}",
                $"Message:  {message}"
            ];
        }
        return [
                $"Expected: {expected}",
                $"Actual:   No Exception was thrown.",
            ];
    }
}

/// <summary>
/// Captures either the value or exception produced by a carefully executed function.
/// </summary>
public sealed class DelayedResult<T> : DelayedResult
{
    private readonly T? value;
    public T? Value
    {
        get
        {
            if (isNone)
                throw new CheckrWasNotExecuted();
            if (value == null)
                throw new DelayedResultNullValueAccessedException();
            return value;
        }
    }

    public bool HasValue
    {
        get
        {
            if (isNone)
                throw new CheckrWasNotExecuted();
            return value != null;
        }
    }

    internal DelayedResult(T value) : base()
        => this.value = value;

    internal DelayedResult(Exception exception) : base(exception) { }

    private readonly bool isNone = false;
    private DelayedResult() { isNone = true; }

    internal static DelayedResult<T> None() => new() { };
}
