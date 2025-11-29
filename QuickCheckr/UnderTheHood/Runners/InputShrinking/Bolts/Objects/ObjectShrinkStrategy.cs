
using System.Reflection;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;

public class ObjectShrinkStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        return HandleProperties(state, check, executionId, key, value, fullKey);
    }

    private static IEnumerable<InputShrinkingClue> HandleProperties<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        if (value == null)
        {
            var clue = InputShrinkingClue.Keep<ObjectShrinkStrategy>(executionId, fullKey, value);
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return [clue];
        }
        var properties = value!.GetType()
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
            var clues = HandleProperty(state, check, executionId, key, value, property, fullKey).ToList();
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
            }
        }

        if (values.Count == 0)
        {
            var clue = InputShrinkingClue.Ignore<ObjectShrinkStrategy>(executionId, fullKey, value);
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return [clue];
        }
        else
        {
            var clue = InputShrinkingClue
                .Keep<ObjectShrinkStrategy>(executionId, fullKey, value,
                    $"{{ {string.Join(", ", values.Select(a => $"{a.PropertyName} : {a.Value}"))} }}");
            clue.AddToCount(shrinkCount);
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return [clue];
        }
    }

    private static IEnumerable<InputShrinkingClue> HandleProperty<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, PropertyInfo propertyInfo, string fullKey)
    {
        var propertyValue = propertyInfo.GetValue(value);
        var swapper = new ObjectLens(propertyInfo);
        using (state.Memory.NestedValue(executionId, swapper))
        {
            foreach (var clue in ShrinkStrategyPicker.Input(state, check, executionId, key, propertyValue, $"{fullKey}.{propertyInfo.Name}"))
                yield return clue;
        }
    }
}