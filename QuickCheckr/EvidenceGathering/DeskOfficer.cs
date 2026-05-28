using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.EvidenceGathering;

public class DeskOfficer
{
    public static bool AreCaseFilesSimilar(CaseFile caseFile, CaseFile other, bool ordered)
    {
        if (caseFile.FailureDeposition.GetFailureDescription() != other.FailureDeposition.GetFailureDescription())
            return false;
        if (caseFile.ExecutionCount != other.ExecutionCount)
            return false;
        return
            ordered switch
            {
                true => Ordered.ExecutionDepositionsAreSimilar(caseFile.ExecutionDepositions, other.ExecutionDepositions),
                false => UnOrdered.ExecutionDepositionsAreSimilar(caseFile.ExecutionDepositions, other.ExecutionDepositions)
            };
    }

    private class UnOrdered
    {
        public static bool ExecutionDepositionsAreSimilar(
            List<ExecutionDeposition> executionDepositions,
            List<ExecutionDeposition> other)
        {
            if (executionDepositions.Count != other.Count)
                return false;
            var counts = executionDepositions
                .Select(ExecutionSignature.From)
                .GroupBy(signature => signature)
                .ToDictionary(group => group.Key, group => group.Count());
            foreach (var signature in other.Select(ExecutionSignature.From))
            {
                if (!counts.TryGetValue(signature, out var count) || count == 0)
                    return false;
                counts[signature] = count - 1;
            }
            return counts.Values.All(count => count == 0);
        }
    }

    private class ExecutionSignature : IEquatable<ExecutionSignature>
    {
        public List<string> Actions { get; }
        public List<string> Inputs { get; }

        private ExecutionSignature(List<string> actions, List<string> inputs)
        {
            Actions = actions;
            Inputs = inputs;
        }

        public static ExecutionSignature From(ExecutionDeposition executionDeposition)
            => new(
                [.. executionDeposition.ActionDepositions.Select(deposition => deposition.Label).OrderBy(label => label)],
                [.. executionDeposition.InputDepositions.Select(deposition => deposition.Label).OrderBy(label => label)]);

        public bool Equals(ExecutionSignature? other)
            => other != null
            && Actions.SequenceEqual(other.Actions)
            && Inputs.SequenceEqual(other.Inputs);

        public override bool Equals(object? obj) => Equals(obj as ExecutionSignature);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var action in Actions)
                hash.Add(action);
            hash.Add("|");
            foreach (var input in Inputs)
                hash.Add(input);
            return hash.ToHashCode();
        }
    }

    private class Ordered
    {
        public static bool ExecutionDepositionsAreSimilar(List<ExecutionDeposition> one, List<ExecutionDeposition> two)
            => !TheseDiffer(one, two, ExecutionDepositionDiffers);

        private static bool ExecutionDepositionDiffers(ExecutionDeposition one, ExecutionDeposition two)
            => TheseDiffer(one.ActionDepositions, two.ActionDepositions, (a, b) => a.Label != b.Label)
            || TheseDiffer(one.InputDepositions, two.InputDepositions, (a, b) => a.Label != b.Label);

        private static bool TheseDiffer<T>(List<T> one, List<T> two, Func<T, T, bool> differs)
        {
            if (one.Count != two.Count)
                return true;
            for (int i = 0; i < one.Count; i++)
            {
                if (differs(one[i], two[i]))
                    return true;
            }
            return false;
        }
    }
}

public static class CaseFileFingerprints
{
    public static string FingerPrint(CaseFile caseFile, bool ordered = false)
    {
        var executions = ordered
            ? OrderedExecutionFingerprints(caseFile.ExecutionDepositions)
            : UnOrderedExecutionFingerprints(caseFile.ExecutionDepositions);

        return string.Join("\n",
        [
            $"Failure: {caseFile.FailureDeposition.GetFailureDescription()}",
            $"ExecutionCount: {caseFile.ExecutionCount}",
            .. executions
        ]);
    }

    private static IEnumerable<string> OrderedExecutionFingerprints(
        List<ExecutionDeposition> executionDepositions)
        => executionDepositions.Select(OrderedExecutionFingerprint);

    private static IEnumerable<string> UnOrderedExecutionFingerprints(
        List<ExecutionDeposition> executionDepositions)
        => executionDepositions
            .Select(UnOrderedExecutionFingerprint)
            .GroupBy(signature => signature)
            .OrderBy(group => group.Key)
            .Select(group => group.Count() == 1
                ? group.Key
                : $"{group.Key} x{group.Count()}");

    private static string OrderedExecutionFingerprint(ExecutionDeposition executionDeposition)
    {
        var actions = string.Join(",",
            executionDeposition.ActionDepositions.Select(deposition => deposition.Label));

        var inputs = string.Join(",",
            executionDeposition.InputDepositions.Select(deposition => deposition.Label));

        return $"Execution: Actions[{actions}] Inputs[{inputs}]";
    }

    private static string UnOrderedExecutionFingerprint(ExecutionDeposition executionDeposition)
    {
        var actions = string.Join(",",
            executionDeposition.ActionDepositions
                .Select(deposition => deposition.Label)
                .OrderBy(label => label));

        var inputs = string.Join(",",
            executionDeposition.InputDepositions
                .Select(deposition => deposition.Label)
                .OrderBy(label => label));

        return $"Execution: Actions[{actions}] Inputs[{inputs}]";
    }
}
