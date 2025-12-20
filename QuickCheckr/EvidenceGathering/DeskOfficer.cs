using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.EvidenceGathering;

public class DeskOfficer
{
    public static bool AreCaseFilesSimilar(CaseFile caseFile, CaseFile other)
        => VerdictIsSimilar(caseFile.Verdict!, other.Verdict!);

    private static bool VerdictIsSimilar(Verdict verdict, Verdict other)
    {
        if (verdict.FailureDeposition.GetFailureDescription() != other.FailureDeposition.GetFailureDescription())
            return false;
        if (verdict.ExecutionCount != other.ExecutionCount)
            return false;
        if (!ExecutionDepositionsAreSimilar(verdict.ExecutionDepositions, other.ExecutionDepositions))
            return false;
        return true;
    }

    private static bool ExecutionDepositionsAreSimilar(
        List<ExecutionDeposition> executionDepositions,
        List<ExecutionDeposition> other)
    {
        if (executionDepositions.Count != other.Count)
            return false;
        int index = 0;
        foreach (var executionDeposition in executionDepositions)
        {
            if (!ExecutionDepositionIsSimilar(executionDepositions[index], other[index]))
                return false;
            index++;
        }
        return true;
    }

    private static bool ExecutionDepositionIsSimilar(
        ExecutionDeposition executionDeposition,
        ExecutionDeposition other)
    {
        if (!ActionDepositionsAreSimilar(executionDeposition.ActionDepositions, other.ActionDepositions))
            return false;
        if (!InputDepositionsAreSimilar(executionDeposition.InputDepositions, other.InputDepositions))
            return false;
        return true;
    }

    private static bool ActionDepositionsAreSimilar(
        List<ActionDeposition> actionDepositions, List<ActionDeposition> other)
    {
        if (actionDepositions.Count != other.Count)
            return false;
        int index = 0;
        foreach (var actionDeposition in actionDepositions)
        {
            if (actionDepositions[index].Label != other[index].Label)
                return false;
            index++;
        }
        return true;
    }

    private static bool InputDepositionsAreSimilar(
        List<InputDeposition> inputDepositions,
         List<InputDeposition> other)
    {
        if (inputDepositions.Count != other.Count)
            return false;
        int index = 0;
        foreach (var inputDeposition in inputDepositions)
        {
            if (inputDepositions[index].Label != other[index].Label)
                return false;
            index++;
        }
        return true;
    }
}