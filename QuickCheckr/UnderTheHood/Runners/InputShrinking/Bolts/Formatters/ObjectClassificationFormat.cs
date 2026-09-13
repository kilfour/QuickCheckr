namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;

internal sealed class ObjectClassificationFormat
{
    public bool HidePropertyNames { get; set; }
    public string LeftBound { get; set; } = "{";
    public string RightBound { get; set; } = "}";
    public string Delimiter { get; set; } = ", ";
    public List<string> Properties { get; set; } = [];
}
