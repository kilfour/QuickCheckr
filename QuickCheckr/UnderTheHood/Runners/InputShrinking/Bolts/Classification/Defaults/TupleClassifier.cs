using System.Reflection;
using QuickCheckr;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class TupleClassifier : Classifier
{
    public override HandlerResult<InputClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(Handle(info, context));
    }

    private static bool CanHandle(ShrinkingInfo info) => info.Type.FullName!.StartsWith("System.ValueTuple");

    private static InputClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (info.Value == null)
        {
            context.Warn(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }
        return HandleProperties(context, info);
    }

    public record CantShrinkNull;

    public record CantShrinkObjectWithNoProperties;


    private static InputClassification HandleProperties(ShrinkingContext context, ShrinkingInfo info)
    {
        var actualType = info.ActualType;
        var properties = actualType
            .GetProperties(MyBinding.Flags)
            .Where(p =>
                p.CanRead &&
                p.GetIndexParameters().Length == 0 &&
                p.SetMethod is not null &&
                p.SetMethod.IsPublic)
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
            values[property.Name] = HandleProperty(context, info, property);
        }

        foreach (var field in fields)
        {
            values[field.Name] = HandleField(context, info, field);
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


    private static InputClassification HandleProperty(ShrinkingContext context, ShrinkingInfo info, PropertyInfo propertyInfo)
    {
        var propertyValue = propertyInfo.GetValue(info.Value);
        var swapper = new ObjectPropertyLens(propertyInfo);
        using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Parent = info, Value = propertyValue, Path = $"{info.Path}.{propertyInfo.Name}", Type = propertyInfo.PropertyType };
            return new ClassifierSelector().Classify(childInfo, context);
        }
    }

    private static InputClassification HandleField(ShrinkingContext context, ShrinkingInfo info, FieldInfo fieldInfo)
    {
        var propertyValue = fieldInfo.GetValue(info.Value);
        var swapper = new ObjectFieldLens(fieldInfo);
        using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Parent = info, Value = propertyValue, Path = $"{info.Path}.{fieldInfo.Name}", Type = fieldInfo.FieldType };
            return new ClassifierSelector().Classify(childInfo, context);
        }
    }
}