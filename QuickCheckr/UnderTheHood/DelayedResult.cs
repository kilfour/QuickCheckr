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

public class DelayedResult<T> : DelayedResult
{
    private readonly T? value;
    public T? Value
    {
        get
        {
            if (isNone)
                throw new CheckrWasNotExecuted();
            if (value == null)
                throw new DelayedResultNullValueAccessed();
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

    public DelayedResult(T value) : base()
        => this.value = value;

    public DelayedResult(Exception exception) : base(exception) { }

    private readonly bool isNone = false;
    private DelayedResult() { isNone = true; }

    public static DelayedResult<T> None() => new() { };
}