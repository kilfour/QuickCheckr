namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Represents an optional string value while preserving the distinction between no value and a null value.
/// </summary>
public sealed record Maybe : Maybe<string>
{
    public static new Maybe Just(string? value)
        => new() { Value = value, HasValue = true };
    public static new Maybe Nothing()
        => new() { };
}

/// <summary>
/// Represents an optional value while preserving the distinction between no value and a present null value.
/// </summary>
public record Maybe<T>
{
    internal Maybe() { }

    public bool HasValue { get; protected set; }
    public T? Value { get; protected set; }
    public static Maybe<T> Just(T? value)
        => new() { Value = value, HasValue = true };
    public static Maybe<T> Nothing()
        => new() { };
}
