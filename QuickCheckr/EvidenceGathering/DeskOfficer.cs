using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.EvidenceGathering;

public class DeskOfficer
{
    public static bool AreCaseFilesSimilar(CaseFile caseFile, CaseFile other, bool ordered = false)
        => VerdictIsSimilar(caseFile.Verdict!, other.Verdict!, ordered);

    private static bool VerdictIsSimilar(Verdict verdict, Verdict other, bool ordered)
    {
        if (verdict.FailureDeposition.GetFailureDescription() != other.FailureDeposition.GetFailureDescription())
            return false;
        if (verdict.ExecutionCount != other.ExecutionCount)
            return false;
        if (ordered)
            return Ordered.ExecutionDepositionsAreSimilar(verdict.ExecutionDepositions, other.ExecutionDepositions);
        else
            return UnOrdered.ExecutionDepositionsAreSimilar(verdict.ExecutionDepositions, other.ExecutionDepositions);
    }
    private class Note
    {
        public List<string> Actions { get; } = [];
        public List<string> Inputs { get; } = [];
        public static List<Note> From(List<ExecutionDeposition> executionDepositions)
        {
            var notepad = new List<Note>();
            foreach (var executionDeposition in executionDepositions)
            {
                for (int i = 0; i < executionDeposition.Times; i++)
                {
                    var note = new Note();
                    foreach (var actionDeposition in executionDeposition.ActionDepositions)
                    {
                        note.Actions.Add(actionDeposition.Label);
                    }
                    foreach (var inputDeposition in executionDeposition.InputDepositions)
                    {
                        note.Inputs.Add(inputDeposition.Label);
                    }
                    notepad.Add(note);
                }
            }
            return notepad;
        }
    }

    private class UnOrdered
    {
        public static bool ExecutionDepositionsAreSimilar(
            List<ExecutionDeposition> executionDepositions,
            List<ExecutionDeposition> other)
        {
            if (executionDepositions.Sum(a => a.Times) != other.Sum(a => a.Times))
                return false;
            var notes = Note.From(executionDepositions);
            var otherNotes = Note.From(other);
            foreach (var note in notes)
            {
                if (!ExecutionDepositionIsSimilar(note, otherNotes))
                    return false;
            }
            return true;
        }

        private static bool ExecutionDepositionIsSimilar(
            Note note,
            List<Note> otherNotes)
        {
            var remaining = ActionDepositionsAreSimilar(note.Actions, otherNotes);
            if (remaining.Count == 0)
                return false;
            remaining = InputDepositionsAreSimilar(note.Inputs, remaining);
            if (remaining.Count == 0)
                return false;
            otherNotes.Remove(remaining.First());
            return true;
        }

        private static List<Note> ActionDepositionsAreSimilar(
            List<string> actions, List<Note> otherNotes)
        {
            var others = otherNotes.Where(a => a.Actions.Count == actions.Count);
            if (!others.Any())
                return [];

            foreach (var action in actions)
            {
                others = others.Where(a => a.Actions.Any(b => b == action));
                if (!others.Any())
                    return [];
            }
            return [.. others];
        }

        private static List<Note> InputDepositionsAreSimilar(
            List<string> inputs,
            List<Note> otherNotes)
        {
            var others = otherNotes.Where(a => a.Inputs.Count == inputs.Count);
            if (!others.Any())
                return [];

            foreach (var input in inputs)
            {
                others = others.Where(a => a.Inputs.Any(b => b == input));
                if (!others.Any())
                    return [];
            }
            return [.. others];
        }
    }

    private class Ordered
    {
        public static bool ExecutionDepositionsAreSimilar(
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
}
