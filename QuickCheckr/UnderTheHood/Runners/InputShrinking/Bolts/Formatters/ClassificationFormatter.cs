using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;

public class ClassificationFormatter
{
    public ManOfWealthAndTaste InputPrinter { get; set; } =
        Please.AllowMe().ToUseNonLinearTime();
    public Dictionary<Type, ObjectClassificationFormat> ObjectClassificationFormats { get; } = [];

    public ObjectClassificationFormatter<T> For<T>()
    {
        var format = new ObjectClassificationFormat();
        ObjectClassificationFormats.Add(typeof(T), format);
        return new(this, format);
    }
}