using System.Linq.Expressions;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;

/// <summary>
/// Configures the shape and member formatting used to render values of <typeparamref name="T"/> in classification reports.
/// </summary>
public sealed class ObjectClassificationFormatter<T>
{
    private readonly ClassificationFormatter classificationFormatter;
    private readonly ObjectClassificationFormat format;

    internal ObjectClassificationFormatter(
        ClassificationFormatter classificationFormatter,
        ObjectClassificationFormat format)
    {
        this.classificationFormatter = classificationFormatter;
        this.format = format;
    }

    public ObjectClassificationFormatter<T> HidePropertyNames()
    {
        format.HidePropertyNames = true;
        return this;
    }

    public ObjectClassificationFormatter<T> HideIrrelevantProperties()
    {
        format.HideIrrelevantProperties = true;
        return this;
    }

    public ObjectClassificationFormatter<T> Bounds(string left, string right)
    {
        format.LeftBound = left;
        format.RightBound = right;
        return this;
    }

    public ObjectClassificationFormatter<T> Delimiter(string delimiter)
    {
        format.Delimiter = delimiter;
        return this;
    }

    public ObjectClassificationFormatter<T> Property<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var name = expression.AsMemberInfo().Name;
        if (!format.Properties.Contains(name)) format.Properties.Add(name);
        return this;
    }

    public ObjectClassificationFormatter<T> Property<TProperty>(
        Expression<Func<T, TProperty>> expression,
        Func<TProperty, string> formatter)
    {
        classificationFormatter.InputPrinter.ToReplace(formatter);
        var name = expression.AsMemberInfo().Name;
        if (!format.Properties.Contains(name)) format.Properties.Add(name);
        return this;
    }
}
