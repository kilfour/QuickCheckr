
using System.Reflection;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;

public class ObjectShrinkStrategy : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
        => HandleProperties(state, check, info);

    public record CantShrinkNull;

    private static IEnumerable<InputShrinkingClue> HandleProperties(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        if (info.Value == null)
        {
            var clue = InputShrinkingClue.Keep<CantShrinkNull>(info.ExecutionId, info.FullKey, info.Value, state.Editor.IntroduceThis(info.Value));
            state.Docket.Record(new InputShrinkingClueFound(clue));
            // TURN HACK BELOW INTO WARNINGS IN ShortTermMemory
            // state.Memory.Tracing = true;
            // state.Memory.StoreTrace($"Can't shrink null value at {info.FullKey}",
            //     () => $"Type: {info.Type.Name}");
            // state.Memory.Tracing = false;
            return [clue];
        }

        var properties = info.Type
            .GetProperties(MyBinding.Flags)
            .Where(p =>
                p.CanRead &&
                p.GetIndexParameters().Length == 0 &&
                p.SetMethod is not null &&
                p.SetMethod.IsPublic)
            .ToList();
        var values = new List<(string PropertyName, string Value)>();
        var shrinkCount = 0;
        foreach (var property in properties)
        {
            var clues = HandleProperty(state, check, info, property).ToList();
            shrinkCount += clues.Sum(a => a.Count);
            var clue = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Replace);
            if (clue != null)
            {
                values.Add((property.Name, clue.Report!));
            }
            else
            {
                clue = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Keep);
                if (clue != null)
                {
                    values.Add((property.Name, clue.Report!));
                }
                else
                {
                    SetPropertyValue(property, info.Value, property.GetValue(info.Value)!);
                }
            }
        }

        if (values.Count == 0)
        {
            var clue = InputShrinkingClue.Ignore<ObjectShrinkStrategy>(info.ExecutionId, info.FullKey, info.Value);
            clue.AddToCount(shrinkCount);
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return [clue];
        }
        else
        {
            var clue = InputShrinkingClue
                .Keep<ObjectShrinkStrategy>(info.ExecutionId, info.FullKey, info.Value,
                    $"{{ {string.Join(", ", values.Select(a => $"{a.PropertyName}: {a.Value}"))} }}");
            clue.AddToCount(shrinkCount);
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return [clue];
        }
    }

    private static IEnumerable<InputShrinkingClue> HandleProperty(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info, PropertyInfo propertyInfo)
    {
        var propertyValue = propertyInfo.GetValue(info.Value);
        var swapper = new ObjectLens(propertyInfo);
        using (state.Memory.NestedValue(info.ExecutionId, swapper))
        {
            var childInfo = info with { Value = propertyValue, FullKey = $"{info.FullKey}.{propertyInfo.Name}", Type = propertyInfo.PropertyType };
            foreach (var clue in new ShrinkStrategyPicker().Shrink(state, check, childInfo))
                yield return clue;
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