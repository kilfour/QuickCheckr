namespace QuickCheckr.UnderTheHood;

public record Maybe
{
    public bool HasValue { get; set; }
    public string? Value { get; set; }
    public static Maybe Just(string? value)
        => new() { Value = value, HasValue = true };

    public static Maybe Nothing()
        => new() { };
}

public record Maybe<T>
{
    public bool HasValue { get; set; }
    public T? Value { get; set; }
    public static Maybe<T> Just(T? value)
        => new() { Value = value, HasValue = true };

    public static Maybe<T> Nothing()
        => new() { };
}