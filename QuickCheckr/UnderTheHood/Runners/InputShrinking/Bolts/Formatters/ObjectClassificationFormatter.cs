using System.Linq.Expressions;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;

public class ObjectClassificationFormatter<T>(
    ClassificationFormatter classificationFormatter,
    ObjectClassificationFormat format)
{
    private readonly ClassificationFormatter classificationFormatter = classificationFormatter;
    private readonly ObjectClassificationFormat format = format;

    public ObjectClassificationFormatter<T> HidePropertyNames()
    {
        format.HidePropertyNames = true;
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
        format.Properties.Add(expression.AsMemberInfo().Name);
        return this;
    }

    public ObjectClassificationFormatter<T> Property<TProperty>(
        Expression<Func<T, TProperty>> expression,
        Func<TProperty, string> formatter)
    {
        classificationFormatter.InputPrinter.ToReplace(formatter);
        format.Properties.Add(expression.AsMemberInfo().Name);
        return this;
    }
}
