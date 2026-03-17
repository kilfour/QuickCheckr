using System.Reflection;
using QuickCheckr;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class ObjectClassifier : Classifier
{
    public override HandlerResult<InputClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(Handle(info, context));
    }

    private static bool CanHandle(ShrinkingInfo info) => info.Type.IsClass;

    private static InputClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (info.Value == null)
        {
            var clue = InputShrinkingClue.Keep<CantShrinkNull>(info.ExecutionId, info.Path, info.Value, context.PrintInput(info.Value));
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            context.State.Memory.StoreWarning(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return new InputRelevant(context.PrintInput(info.Value));
        }
        return HandleProperties(context, info);
    }

    public record CantShrinkNull;

    public record CantShrinkObjectWithNoProperties;


    private static InputClassification HandleProperties(ShrinkingContext context, ShrinkingInfo info)
    {
        var actualType = info.Value == null ? info.Type : info.Value.GetType();
        var properties = actualType
            .GetProperties(MyBinding.Flags)
            .Where(p =>
                p.CanRead &&
                p.GetIndexParameters().Length == 0 &&
                p.SetMethod is not null &&
                p.SetMethod.IsPublic)
            .ToList();
        if (properties.Count == 0)
        {
            var clue = InputShrinkingClue.Keep<CantShrinkObjectWithNoProperties>(info.ExecutionId, info.Path, info.Value, context.PrintInput(info.Value));
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            context.State.Memory.StoreWarning(info.Path, () => new NoPropertiesWarning(info.Type, info.Path));
            return new InputRelevant(context.PrintInput(info.Value));
        }
        var values = new Dictionary<string, InputClassification>();
        foreach (var property in properties)
        {
            values[property.Name] = HandleProperty(context, info, property);
        }

        if (values.Any(a => a.Value is InputRelevant))
        {
            var clue = InputShrinkingClue
                .Keep<ObjectClassifier>(info.ExecutionId, info.Path, info.Value,
                    $"{{ {string.Join(", ", values.Select(a => $"{a.Key}: {a.Value}"))} }}");
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            if (context.State.ClassificationFormatter.ObjectClassificationFormats.TryGetValue(actualType, out var format))
            {
                var keys = format.Properties.Select(a => a.Name).ToList();
                var keySet = new HashSet<string>(keys);
                var ordered =
                    keys
                        .Where(values.ContainsKey)
                        .Select(k => KeyValuePair.Create(k, values[k]))
                        .Concat(
                            values
                                .Where(kvp => !keySet.Contains(kvp.Key))
                                .Select(kvp => kvp))
                        .ToDictionary();
                return new ObjectRelevant(values, GetCustomReport(format, ordered));
            }
            return new ObjectRelevant(values, GetReport(values));
        }
        else
        {
            var clue = InputShrinkingClue.Ignore<ObjectClassifier>(info.ExecutionId, info.Path, info.Value);
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            return new ObjectAssumedIrrelevant(values);
        }
    }

    private static string GetCustomReport(
        ObjectClassificationFormat objectFormat,
        Dictionary<string, InputClassification> members) =>
        $"{objectFormat.LeftBound}{string.Join(objectFormat.Delimiter,
            members
                .Select(a => CustomPropertyReport(objectFormat, a)))}{objectFormat.RightBound}";

    private static string CustomPropertyReport(
        ObjectClassificationFormat objectFormat,
        KeyValuePair<string, InputClassification> kvp) =>
        objectFormat.HidePropertyNames
            ? GetCustomClassificationReport(kvp.Value)
            : $"{kvp.Key}: {GetCustomClassificationReport(kvp.Value)}";

    private static string GetCustomClassificationReport(InputClassification classification)
    {
        if (classification is InputRelevant relevant)
            return relevant.Report;
        return "_";
    }

    private static string GetReport(Dictionary<string, InputClassification> members) =>
        $"{{ {string.Join(", ",
            members.Where(a => a.Value is InputRelevant)
                .Select(kvp => $"{kvp.Key}: {(kvp.Value as InputRelevant)!.Report}"))} }}";

    private static InputClassification HandleProperty(ShrinkingContext context, ShrinkingInfo info, PropertyInfo propertyInfo)
    {
        var propertyValue = propertyInfo.GetValue(info.Value);
        var swapper = new ObjectLens(propertyInfo);
        using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Value = propertyValue, Path = $"{info.Path}.{propertyInfo.Name}", Type = propertyInfo.PropertyType };
            return new ClassifierSelector().Classify(childInfo, context);
        }
    }
}