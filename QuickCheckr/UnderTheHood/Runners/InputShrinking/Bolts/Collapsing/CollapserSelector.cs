namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

internal static class CollapserSelector
{
    public static ShrinkingInfo Collapse(ShrinkingInfo info, ShrinkingContext context)
    {
        var shrinkers = context.State.Memory.GetShrinkers(
            info.ExecutionId,
            info.Key,
            info.Type);
        var shrinker = shrinkers?
            .Where(candidate => candidate.Collapsers.Length != 0)
            .FirstOrDefault(candidate => candidate.Condition(info));
        if (shrinker is null || info.Value is null)
            return info;

        var current = context.State.Memory.GetCurrentInputValue(info.ExecutionId, info.Key);
        if (current is null)
            return info with { Value = null };

        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance) { current };
        var collapsed = true;
        while (collapsed)
        {
            collapsed = false;
            foreach (var option in shrinker.Collapsers)
            {
                var candidate = option.Select(current);
                if (candidate is null || !visited.Add(candidate))
                    continue;
                if (!context.Verifier.RunFailed(info.ExecutionId, info.Key, candidate))
                    continue;

                context.State.Memory.Commit(info.ExecutionId, info.Key, candidate);
                context.CollapseCount++;
                current = candidate;
                collapsed = true;
                break;
            }
        }

        return info with { Value = current };
    }
}
