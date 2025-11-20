namespace QuickCheckr.UnderTheHood.Proceedings;

public record InputDeposition
{
    public string Label { get; }
    public object Value { get; }

    public InputDeposition(string label, object value)
    {
        Label = label;
        Value = value;
    }
};