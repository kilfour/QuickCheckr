using QuickCheckr;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;
using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

internal static class CustomizedPrinting
{
    public static string GetReduxReport(
        ObjectClassificationFormat format,
        Dictionary<string, InputClassification> values)
            => Report(format, values, true);

    public static string GetReport(
        ObjectClassificationFormat format,
        Dictionary<string, InputClassification> values)
            => Report(format, values, false);

    private static string Report(
        ObjectClassificationFormat format,
        Dictionary<string, InputClassification> values,
        bool useRedux)
    {
        var keySet = new HashSet<string>(format.Properties);
        var ordered =
            format.Properties
                .Where(values.ContainsKey)
                .Select(k => KeyValuePair.Create(k, values[k]))
                .Concat(
                    values
                        .Where(kvp => !keySet.Contains(kvp.Key))
                        .Select(kvp => kvp))
                .ToDictionary();
        return $"{format.LeftBound}{string.Join(format.Delimiter,
            ordered
                .Select(a => PropertyReport(format, a, useRedux)))}{format.RightBound}";
    }

    private static string PropertyReport(
        ObjectClassificationFormat format,
        KeyValuePair<string, InputClassification> kvp,
        bool useRedux) =>
        format.HidePropertyNames
            ? GetClassificationReport(kvp.Value, useRedux)
            : $"{kvp.Key}: {GetClassificationReport(kvp.Value, useRedux)}";

    private static string GetClassificationReport(InputClassification classification, bool useRedux)
    {
        if (classification is InputRelevant relevant)
        {
            if (useRedux && relevant.Redux.HasValue)
                return relevant.Redux.Value!.Report;
            return relevant.Report;
        }
        return "_";
    }
}
