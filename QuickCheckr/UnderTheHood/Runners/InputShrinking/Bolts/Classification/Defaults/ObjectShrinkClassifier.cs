using System.Reflection;
using QuickCheckr;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class ObjectShrinkClassifier : Classifier
{
    public override HandlerResult<InputShrinkingClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(Handle(info, context));
    }

    private static bool CanHandle(ShrinkingInfo info) => info.Type.IsClass;

    private static InputShrinkingClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (info.Value == null)
        {
            var clue = InputShrinkingClue.Keep<CantShrinkNull>(info.ExecutionId, info.Path, info.Value, context.State.InputPrinter.IntroduceThis(info.Value));
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            context.State.Memory.StoreWarning(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return new InputRelevant(context.State.InputPrinter.IntroduceThis(info.Value));
        }
        return HandleProperties(context, info);
    }

    public record CantShrinkNull;

    public record CantShrinkObjectWithNoProperties;


    private static InputShrinkingClassification HandleProperties(ShrinkingContext context, ShrinkingInfo info)
    {
        var properties = info.Type
            .GetProperties(MyBinding.Flags)
            .Where(p =>
                p.CanRead &&
                p.GetIndexParameters().Length == 0 &&
                p.SetMethod is not null &&
                p.SetMethod.IsPublic)
            .ToList();
        if (properties.Count == 0)
        {
            var clue = InputShrinkingClue.Keep<CantShrinkObjectWithNoProperties>(info.ExecutionId, info.Path, info.Value, context.State.InputPrinter.IntroduceThis(info.Value));
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            context.State.Memory.StoreWarning(info.Path, () => new NoPropertiesWarning(info.Type, info.Path));
            return new InputRelevant(context.State.InputPrinter.IntroduceThis(info.Value));
        }
        var values = new Dictionary<string, InputShrinkingClassification>();
        foreach (var property in properties)
        {
            values[property.Name] = HandleProperty(context, info, property);
        }

        if (values.Any(a => a.Value is InputRelevant))
        {
            var clue = InputShrinkingClue
                .Keep<ObjectShrinkClassifier>(info.ExecutionId, info.Path, info.Value,
                    $"{{ {string.Join(", ", values.Select(a => $"{a.Key}: {a.Value}"))} }}");
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            return new ObjectRelevant(values);
        }
        else
        {
            var clue = InputShrinkingClue.Ignore<ObjectShrinkClassifier>(info.ExecutionId, info.Path, info.Value);
            context.State.Docket.Record(new InputShrinkingClueFound(clue));
            return new ObjectAssumedIrrelevant(values);
        }
    }

    private static InputShrinkingClassification HandleProperty(ShrinkingContext context, ShrinkingInfo info, PropertyInfo propertyInfo)
    {
        var propertyValue = propertyInfo.GetValue(info.Value);
        var swapper = new ObjectLens(propertyInfo);
        using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Value = propertyValue, Path = $"{info.Path}.{propertyInfo.Name}", Type = propertyInfo.PropertyType };
            return new ShrinkClassifierSelector().Classify(childInfo, context);
        }
    }
}