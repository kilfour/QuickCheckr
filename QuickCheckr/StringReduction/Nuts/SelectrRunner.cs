namespace QuickCheckr.StringReduction.Nuts;

public class SelectrRun(SelectrOf<Selection> selectr, string text)
{
    private readonly SelectrState state = new(text);
    private int searchStart;
    private bool finished = text.Length == 0;
    private StateCheckpoint candidateCheckpoint;
    private int candidateSearchStart;
    private bool hasCandidateCheckpoint;

    public string Next()
    {
        hasCandidateCheckpoint = false;

        while (searchStart < state.Text.Length)
        {
            var attemptStart = searchStart;
            var checkpoint = state.Capture();
            state.StartAttempt(attemptStart);

            var result = selectr(state);
            if (!result.Succeeded)
            {
                state.Restore(checkpoint);
                searchStart = attemptStart + 1;
                continue;
            }

            candidateCheckpoint = checkpoint;
            candidateSearchStart = attemptStart;
            hasCandidateCheckpoint = true;

            var textChanged = state.Text != checkpoint.Text;
            if (!textChanged)
            {
                var nextSearchStart = Math.Max(attemptStart + 1, state.Index);
                state.Restore(checkpoint);
                hasCandidateCheckpoint = false;
                searchStart = nextSearchStart;
                continue;
            }

            searchStart = attemptStart;
            finished = state.Index >= state.Text.Length;

            return state.Text;
        }

        finished = true;
        return state.Text;
    }

    public void RejectCandidate()
    {
        if (!hasCandidateCheckpoint)
            return;

        state.Restore(candidateCheckpoint);
        searchStart = candidateSearchStart + 1;
        finished = searchStart >= state.Text.Length;
        hasCandidateCheckpoint = false;
    }

    public bool EndOfText => finished;

    public bool Changed => state.Changed;
}
