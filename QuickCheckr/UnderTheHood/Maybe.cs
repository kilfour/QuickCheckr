namespace QuickCheckr.UnderTheHood;

public record Maybe : Maybe<string>
{
    public static new Maybe Just(string? value)
        => new() { Value = value, HasValue = true };
    public static new Maybe Nothing()
        => new() { };
}

public record Maybe<T>
{
    public bool HasValue { get; protected set; }
    public T? Value { get; protected set; }
    public static Maybe<T> Just(T? value)
        => new() { Value = value, HasValue = true };
    public static Maybe<T> Nothing()
        => new() { };
}
