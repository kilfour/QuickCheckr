
using System.Reflection;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;

public class ObjectShrinkStrategy2
{
    public InputShrinkingClassification Classify(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        if (info.Value == null)
        {
            var clue = InputShrinkingClue.Keep<CantShrinkNull>(info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
            state.Docket.Record(new InputShrinkingClueFound(clue));
            state.Memory.StoreWarning(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return new InputRelevant(state.InputPrinter.IntroduceThis(info.Value));
        }
        return HandleProperties(state, check, info);
    }

    public record CantShrinkNull;

    public record CantShrinkObjectWithNoProperties;


    private static InputShrinkingClassification HandleProperties(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
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
            var clue = InputShrinkingClue.Keep<CantShrinkObjectWithNoProperties>(info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
            state.Docket.Record(new InputShrinkingClueFound(clue));
            state.Memory.StoreWarning(info.Path, () => new NoPropertiesWarning(info.Type, info.Path));
            return new InputRelevant(state.InputPrinter.IntroduceThis(info.Value));
        }
        var values = new Dictionary<string, InputShrinkingClassification>();
        foreach (var property in properties)
        {
            values[property.Name] = HandleProperty(state, check, info, property);
        }

        if (values.Any(a => a.Value is InputRelevant))
        {
            var clue = InputShrinkingClue
                .Keep<ObjectShrinkStrategy>(info.ExecutionId, info.Path, info.Value,
                    $"{{ {string.Join(", ", values.Select(a => $"{a.Key}: {a.Value}"))} }}");
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return new ObjectRelevant(values);
        }
        else
        {
            var clue = InputShrinkingClue.Ignore<ObjectShrinkStrategy>(info.ExecutionId, info.Path, info.Value);
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return new ObjectAssumedIrrelevant(values);
        }
    }

    private static InputShrinkingClassification HandleProperty(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info, PropertyInfo propertyInfo)
    {
        var propertyValue = propertyInfo.GetValue(info.Value);
        var swapper = new ObjectLens(propertyInfo);
        using (state.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Value = propertyValue, Path = $"{info.Path}.{propertyInfo.Name}", Type = propertyInfo.PropertyType };
            return new ShrinkStrategyPicker().Classify(state, check, childInfo);
        }
    }

    //duplicate from ObjectLens
    private static void SetPropertyValue(PropertyInfo propertyInfo, object target, object value)
    {
        var prop = propertyInfo;
        if (!prop.CanWrite)
            prop = propertyInfo.DeclaringType!.GetProperty(propertyInfo.Name);

        if (prop != null && prop.CanWrite)
            prop.SetValue(target, value, null);
    }
}