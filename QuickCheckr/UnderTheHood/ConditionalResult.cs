namespace QuickCheckr.UnderTheHood;

public class ConditionalResult<T>
{
    private readonly bool notExecuted = false;
    private readonly T? value;
    public T? Value
    {
        get
        {
            if (notExecuted)
                throw new CheckrWasNotExecuted();
            return value;
        }
    }
    private ConditionalResult() { notExecuted = true; }
    public ConditionalResult(T? value) => this.value = value;
    public static ConditionalResult<T> NotExecuted() => new() { };
}
