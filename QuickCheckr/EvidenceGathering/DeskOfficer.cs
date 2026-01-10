using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

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

    private class UnOrdered
    {
        public static bool ExecutionDepositionsAreSimilar(
            List<ExecutionDeposition> executionDepositions,
            List<ExecutionDeposition> other)
        {
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

    private class Note
    {
        public List<string> Actions { get; } = [];
        public List<string> Inputs { get; } = [];
        public static List<Note> From(List<ExecutionDeposition> executionDepositions)
        {
            var notepad = new List<Note>();
            foreach (var executionDeposition in executionDepositions)
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
            return notepad;
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
