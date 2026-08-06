using System.Reflection;
using QuickCheckr;
using QuickCheckr.UnderTheHood.Dispatching;
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
            context.Warn(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }
        return HandleProperties(context, info);
    }

    private static InputClassification HandleProperties(ShrinkingContext context, ShrinkingInfo info)
    {
        var actualType = info.ActualType;
        var properties = GetDefaultProperties(actualType);
        if (properties.Count == 0)
        {
            context.Warn(info.Path, () => new NoPropertiesWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }
        var values = new Dictionary<string, InputClassification>();
        foreach (var property in properties)
        {
            values[property.Name] = ClassifyProperty(context, info, property);
        }

        return ClassifyMembers(actualType, context, values);
    }

    internal static IReadOnlyList<PropertyInfo> GetReadableProperties(Type actualType) =>
        actualType
            .GetProperties(MyBinding.Flags)
            .Where(p =>
                p.CanRead &&
                p.GetIndexParameters().Length == 0)
            .ToList();

    internal static IReadOnlyList<PropertyInfo> GetDefaultProperties(Type actualType) =>
        GetReadableProperties(actualType)
            .Where(CanTraverse)
            .ToList();

    internal static bool CanTraverse(PropertyInfo propertyInfo) =>
        propertyInfo.SetMethod is not null &&
        propertyInfo.SetMethod.IsPublic;

    internal static InputClassification ClassifyMembers(
        Type actualType,
        ShrinkingContext context,
        Dictionary<string, InputClassification> values)
    {
        if (values.Any(a => a.Value is InputRelevant))
        {
            if (context.State.ClassificationFormatter.ObjectClassificationFormats.TryGetValue(actualType, out var format))
                return new ObjectRelevant(values, CustomizedPrinting.GetReport(format, values));
            return new ObjectRelevant(values, GetReport(values));
        }
        else
        {
            return new ObjectAssumedIrrelevant(values);
        }
    }

    private static string GetReport(Dictionary<string, InputClassification> members) =>
        $"{{ {string.Join(", ",
            members.Where(a => a.Value is InputRelevant)
                .Select(kvp => $"{kvp.Key}: {(kvp.Value as InputRelevant)!.Report}"))} }}";

    internal static InputClassification ClassifyProperty(
        ShrinkingContext context,
        ShrinkingInfo info,
        PropertyInfo propertyInfo)
    {
        var parent = context.State.Memory.GetCurrentInputValue(
            info.ExecutionId,
            info.Key);
        var propertyValue = propertyInfo.GetValue(parent);
        var swapper = new ObjectPropertyLens(propertyInfo);
        using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with
            {
                Parent = info with { Value = parent },
                Value = propertyValue,
                Path = $"{info.Path}.{propertyInfo.Name}",
                Type = propertyInfo.PropertyType
            };
            return new ClassifierSelector().Classify(childInfo, context);
        }
    }

    internal static InputClassification RetainProperty(
        ShrinkingContext context,
        ShrinkingInfo info,
        PropertyInfo propertyInfo)
    {
        var parent = context.State.Memory.GetCurrentInputValue(
            info.ExecutionId,
            info.Key);
        return InputRelevant.Create(
            context.PrintInput(propertyInfo.GetValue(parent)));
    }
}
