using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;

internal static class TraversalSelector
{
    public static InputClassification Traverse(
        ShrinkingInfo info,
        ShrinkingContext context,
        IEnumerable<ITraversalOption> options)
    {
        if (info.Value is null)
        {
            context.Warn(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }

        var configured = options.ToList();
        var configuredByName = configured.ToDictionary(option => option.MemberName);
        var readableProperties = ObjectClassifier.GetReadableProperties(info.ActualType);
        var defaultProperties = readableProperties
            .Where(ObjectClassifier.CanTraverse)
            .ToList();
        var defaultNames = defaultProperties
            .Select(property => property.Name)
            .ToHashSet();
        var configuredNames = configured
            .Select(option => option.MemberName)
            .ToHashSet();
        var members = new Dictionary<string, InputClassification>();

        foreach (var property in defaultProperties)
        {
            members[property.Name] = configuredByName.TryGetValue(property.Name, out var option)
                ? ClassifyOption(info, context, option)
                : ObjectClassifier.ClassifyProperty(context, info, property);
        }

        foreach (var option in configured.Where(option => !defaultNames.Contains(option.MemberName)))
            members[option.MemberName] = ClassifyOption(info, context, option);

        foreach (var property in readableProperties.Where(property =>
                     !ObjectClassifier.CanTraverse(property) &&
                     !configuredNames.Contains(property.Name)))
        {
            members[property.Name] = ObjectClassifier.RetainProperty(
                context,
                info,
                property);
        }

        return ObjectClassifier.ClassifyMembers(info.ActualType, context, members);
    }

    private static InputClassification ClassifyOption(
        ShrinkingInfo info,
        ShrinkingContext context,
        ITraversalOption option)
    {
        var parent = context.State.Memory.GetCurrentInputValue(
            info.ExecutionId,
            info.Key);
        var memberValue = option.Get(parent!);
        var lens = new MemoryLens(
            target => option.Get(target)!,
            option.Replace);

        using (context.State.Memory.NestedValue(info.ExecutionId, lens))
        {
            var childInfo = new ShrinkingInfo(
                info.ExecutionId,
                info.Key,
                memberValue,
                $"{info.Path}.{option.MemberName}",
                option.MemberType)
            {
                Parent = info with { Value = parent }
            };
            return new ClassifierSelector().Classify(childInfo, context);
        }
    }
}
