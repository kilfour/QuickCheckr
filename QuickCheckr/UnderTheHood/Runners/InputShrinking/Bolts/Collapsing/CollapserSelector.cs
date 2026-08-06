using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

public static class CollapserSelector
{
    public static InputClassification Collapse(
        ShrinkingInfo info,
        ShrinkingContext context,
        IEnumerable<ICollapseOption> options)
    {
        if (info.Value is null)
        {
            context.Warn(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }

        var members = new Dictionary<string, InputClassification>();
        foreach (var option in options)
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
                members[option.MemberName] =
                    new ClassifierSelector().Classify(childInfo, context);
            }
        }

        return members.Values.Any(value => value is InputRelevant)
            ? new ObjectRelevant(members, Report(members))
            : new ObjectAssumedIrrelevant(members);
    }

    private static string Report(
        IReadOnlyDictionary<string, InputClassification> members) =>
        $"{{ {string.Join(", ", members
            .Where(member => member.Value is InputRelevant)
            .Select(member =>
                $"{member.Key}: {((InputRelevant)member.Value).Report}"))} }}";
}
