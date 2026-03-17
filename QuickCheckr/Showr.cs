using System.Linq.Expressions;
using System.Reflection;
using QuickCheckr.UnderTheHood;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr;

public class PropertyClassificationFormat
{
    public string Name { get; set; } = "";
    public Func<object, string>? Formatter { get; set; } = null;
}

public class ObjectClassificationFormat
{
    public bool HidePropertyNames { get; set; }
    public string LeftBound { get; set; } = "{";
    public string RightBound { get; set; } = "}";
    public string Delimiter { get; set; } = ", ";
    public List<PropertyClassificationFormat> Properties { get; set; } = [];
}

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
        format.Properties.Add(
            new PropertyClassificationFormat
            {
                Name = expression.AsMemberInfo().Name
            });
        return this;
    }

    public ObjectClassificationFormatter<T> Property<TProperty>(
        Expression<Func<T, TProperty>> expression,
        Func<TProperty, string> formatter)
    {
        classificationFormatter.InputPrinter.ToReplace(formatter);
        format.Properties.Add(
            new PropertyClassificationFormat
            {
                Name = expression.AsMemberInfo().Name,
                //Formatter = value => formatter((TProperty)value)
            });
        return this;
    }
}

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

public static partial class Showr
{

    // public static CheckrOf<Case> ForClassification(Func<ClassificationFormatter, ClassificationFormatter> config) =>
    public static CheckrOf<Case> ForClassification(Action<ClassificationFormatter> config) =>
        state =>
        {
            if (state.EvidenceRecorder.IsReplay)
                return CheckrResult.CaseOnly(state);
            config(state.ClassificationFormatter);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForTrace(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Editor = config();
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForInput() =>
        state =>
        {
            state.Memory.StoreInputShowr(() => Please.AllowMe().ToUseNonLinearTime());
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForInput(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Memory.StoreInputShowr(config);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForAll(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Editor = config();
            state.Memory.StoreInputShowr(config);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> None() =>
        state =>
        {
            state.Editor = Please.AllowMe().ToUseNonLinearTime();
            state.Memory.StoreInputShowr(null!);
            return CheckrResult.CaseOnly(state);
        };
}