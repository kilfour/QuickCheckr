namespace QuickCheckr.UnderTheHood;

public class DelayedResult
{
    public bool Threw { get; init; }
    public Exception? Exception { get; init; }

    public DelayedResult()
    {
        Threw = false;
        Exception = null;
    }

    public DelayedResult(Exception exception)
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
}

public sealed class DelayedResult<T> : DelayedResult
{
    public T? Value { get; }

    public DelayedResult(T value) : base()
    {
        Value = value;
    }

    public DelayedResult(Exception exception) : base(exception)
    {
        Value = default;
    }
}