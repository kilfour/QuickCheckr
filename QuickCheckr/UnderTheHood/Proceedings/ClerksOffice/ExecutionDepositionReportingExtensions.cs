using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class ExecutionDepositionReportingExtensions
{
    public static IEnumerable<WarningDeposition> GetWarningDepositionsForReport(this ExecutionDeposition executionDeposition)
    {
        foreach (var warningDeposition in executionDeposition.WarningDepositions.Where(a => a.Warning is not EmptyCollectionWarning))
        {
            yield return warningDeposition;
        }
        var emptyCollectionWarnings = GetEmptyCollectionWarnings(executionDeposition);
        if (!emptyCollectionWarnings.Any())
            yield break;
        if (emptyCollectionWarnings.Count() == 1)
        {
            yield return emptyCollectionWarnings.Single();
            yield break;
        }

        var info = new List<string>();
        foreach (var group in emptyCollectionWarnings.Select(a => a.Warning).Cast<EmptyCollectionWarning>().GroupBy(a => a.Key))
        {
            if (group.Count() == 1)
                info.Add("       " + group.Key);
            else
            {
                var index = GetIndexOfFirstChange(group.Select(a => a.Path));
                info.Add($"       {GetRoot(group, index)}: {string.Join(", ", group.Select(a => a.Path[index..]))}");
            }
        }
        yield return new WarningDeposition(new CompositeEmptyCollectionWarning(info));
    }

    private static IEnumerable<WarningDeposition> GetEmptyCollectionWarnings(ExecutionDeposition executionDeposition)
        => executionDeposition.WarningDepositions.Where(a => a.Warning is EmptyCollectionWarning);


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