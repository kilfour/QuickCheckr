
namespace QuickCheckr.UnderTheHood.Proceedings;

public record InputDeposition(
    bool Labeled,
    string Label,
    object Value,
    Maybe Original,
    Maybe Redux)
{
    public int MaxLabelLength()
        => Math.Max(Label.Length, Redux.HasValue ? 5 : 0);
}

public record Maybe
{
    public bool HasValue { get; set; }
    public string? Value { get; set; }
    public static Maybe Just(string? value)
        => new() { Value = value, HasValue = true };

    public static Maybe Nothing()
        => new() { };
}