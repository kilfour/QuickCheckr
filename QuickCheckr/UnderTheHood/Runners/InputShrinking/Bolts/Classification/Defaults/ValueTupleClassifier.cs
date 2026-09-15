using System.Reflection;
using QuickCheckr;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

internal sealed class ValueTupleClassifier : Classifier
{
    public override async ValueTask<HandlerResult<InputClassification>> TryHandleAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(await HandleAsync(info, context).ConfigureAwait(false));
    }

    private static bool CanHandle(ShrinkingInfo info) =>
        info.Type.FullName!.StartsWith("System.ValueTuple");

    private static async ValueTask<InputClassification> HandleAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        if (info.Value == null)
        {
            context.Warn(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }
        return await HandlePropertiesAsync(context, info).ConfigureAwait(false);
    }

    private static async ValueTask<InputClassification> HandlePropertiesAsync(ShrinkingContext context, ShrinkingInfo info)
    {
        var actualType = info.ActualType;
        var properties = actualType
            .GetProperties(MyBinding.Flags)
            .Where(p =>
                p.CanRead &&
                p.GetIndexParameters().Length == 0 &&
                p.SetMethod is not null &&
                p.SetMethod.IsPublic)
            .Concat(actualType.GetProperties(MyBinding.Flags).Where(a => a.Name.StartsWith("Item")))
            .ToList();
        var fields = true // includeFields
            ? actualType
                .GetFields(MyBinding.Flags)
                .Where(f =>
                    f.IsPublic &&
                    !f.IsInitOnly &&
                    !f.IsLiteral)
                .ToList()
            : [];

        if (properties.Count + fields.Count == 0)
        {
            context.Warn(info.Path, () => new NoPropertiesWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }
        var values = new Dictionary<string, InputClassification>();
        foreach (var property in properties)
        {
            values[property.Name] = await HandlePropertyAsync(context, info, property).ConfigureAwait(false);
        }

        foreach (var field in fields)
        {
            values[field.Name] = await HandleFieldAsync(context, info, field).ConfigureAwait(false);
        }
        if (values.Any(a => a.Value is InputRelevant))
        {
            if (context.State.ClassificationFormatter.ObjectClassificationFormats.TryGetValue(actualType, out var format))
                return new TupleRelevant(values, CustomizedPrinting.GetReport(format, values));
            return new TupleRelevant(values, GetReport(values));
        }
        else
        {
            return new ObjectAssumedIrrelevant(values);
        }
    }

    private static string GetReport(Dictionary<string, InputClassification> members) =>
        $"( {string.Join(", ",
            members
                .Select(kvp => $"{GetValue(kvp)}"))} )";

    private static string GetValue(KeyValuePair<string, InputClassification> kvp) =>
        kvp.Value switch
        {
            InputRelevant => (kvp.Value as InputRelevant)!.Report,
            _ => "_"
        };


    private static async ValueTask<InputClassification> HandlePropertyAsync(ShrinkingContext context, ShrinkingInfo info, PropertyInfo propertyInfo)
    {
        var propertyValue = propertyInfo.GetValue(info.Value);
        var swapper = new ObjectPropertyLens(propertyInfo);
        using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Parent = info, Value = propertyValue, Path = $"{info.Path}.{propertyInfo.Name}", Type = propertyInfo.PropertyType };
            return await new ClassifierSelector().ClassifyAsync(childInfo, context).ConfigureAwait(false);
        }
    }

    private static async ValueTask<InputClassification> HandleFieldAsync(ShrinkingContext context, ShrinkingInfo info, FieldInfo fieldInfo)
    {
        var propertyValue = fieldInfo.GetValue(info.Value);
        var swapper = new ObjectFieldLens(fieldInfo);
        using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Parent = info, Value = propertyValue, Path = $"{info.Path}.{fieldInfo.Name}", Type = fieldInfo.FieldType };
            return await new ClassifierSelector().ClassifyAsync(childInfo, context).ConfigureAwait(false);
        }
    }
}
