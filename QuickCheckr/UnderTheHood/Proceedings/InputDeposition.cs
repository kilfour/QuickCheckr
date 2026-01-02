namespace QuickCheckr.UnderTheHood.Proceedings;

public record InputDeposition(
    bool Labeled,
    string Label,
    object Value,
    string? Original = null,
    object? Redux = null);