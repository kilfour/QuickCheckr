namespace QuickCheckr.StringReduction.Nuts;

/// <summary>
/// Executes a completed Stringr script and searches the input for successive reduction candidates.
/// Use <see cref="Next"/> until <see cref="EndOfText"/> indicates that no further candidates remain.
/// </summary>
public sealed class SelectrRun(SelectrOf<Selection> selectr, string text)
{
    private readonly SelectrState state = new(text);
    private int searchStart;
    private bool finished = text.Length == 0;
    private StateCheckpoint candidateCheckpoint;
    private int candidateSearchStart;
    private bool hasCandidateCheckpoint;

    /// <summary>
    /// Returns the next changed candidate, or the current text after the search is exhausted.
    /// Candidates are cumulative unless the current candidate is rejected.
    /// </summary>
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

    /// <summary>
    /// Rejects the most recently returned candidate and resumes searching from the preceding text.
    /// Used by reducer integration when a candidate causes the failing property to pass.
    /// </summary>
    public void RejectCandidate()
    {
        if (!hasCandidateCheckpoint)
            return;

        state.Restore(candidateCheckpoint);
        searchStart = candidateSearchStart + 1;
        finished = searchStart >= state.Text.Length;
        hasCandidateCheckpoint = false;
    }

    /// <summary>
    /// Indicates that the selector has no further candidate to produce.
    /// </summary>
    public bool EndOfText => finished;

    /// <summary>
    /// Indicates whether the current selection attempt performed an editing operation.
    /// </summary>
    public bool Changed => state.Changed;
}
