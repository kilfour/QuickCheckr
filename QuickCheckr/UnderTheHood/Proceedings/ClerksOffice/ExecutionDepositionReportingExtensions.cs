using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class ExecutionDepositionReportingExtensions
{
    public static void GetWarningDepositions(this ExecutionDeposition executionDeposition, ExecutionMemory executionMemory)
    {
        foreach (var warning in executionMemory.GetWarnings().Where(a => a is not EmptyCollectionWarning))
        {
            executionDeposition.AddWarningDeposition(new WarningDeposition(warning));
        }
        var emptyCollectionWarnings = GetEmptyCollectionWarnings(executionMemory);
        if (!emptyCollectionWarnings.Any())
            return;
        if (emptyCollectionWarnings.Count() == 1)
        {
            executionDeposition.AddWarningDeposition(new WarningDeposition(emptyCollectionWarnings.Single()));
            return;
        }

        var info = new List<string>();
        foreach (var group in emptyCollectionWarnings.GroupBy(a => a.Key))
        {
            if (group.Count() == 1)
                info.Add("       " + group.Key);
            else
            {
                var index = GetIndexOfFirstChange(group.Select(a => a.Path));
                info.Add($"       {GetRoot(group, index)}: {string.Join(", ", group.Select(a => a.Path[index..]))}");
            }
        }
        executionDeposition.AddWarningDeposition(new WarningDeposition(new CompositeEmptyCollectionWarning(info)));
    }

    private static IEnumerable<EmptyCollectionWarning> GetEmptyCollectionWarnings(ExecutionMemory shortTermMemory)
    {
        return shortTermMemory.GetWarnings().Where(a => a is EmptyCollectionWarning).Cast<EmptyCollectionWarning>();
    }

    private static string GetRoot(IGrouping<string, EmptyCollectionWarning> group, int index)
    {
        var root = group.First().Path[..index];
        if (root.EndsWith('.'))
            root = root[..^1];
        return root;
    }

    private static int GetIndexOfFirstChange(IEnumerable<string> paths)
    {
        var current = "";
        foreach (var path in paths)
        {
            if (current == string.Empty)
            {
                current = path;
                continue;
            }
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != path[i])
                {
                    current = current[..i];
                    break;
                }
            }

        }
        return current.Length;
    }
}