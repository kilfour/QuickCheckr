using QuickCheckr.FilingCabinet;

namespace QuickCheckr.UnderTheHood;


internal sealed class Deliberator(
    Func<CaseFile, int> scoringFunction,
    int? deliberationTarget = null)
{
    private record DeliberatedCaseFile(CaseFile CaseFile, int Score);

    private DeliberatedCaseFile? current = null;

    public bool HasCaseFile => current is not null;

    public void Check(CaseFile caseFile)
    {
        var score = scoringFunction(caseFile);
        if (current is null)
        {
            current = new(caseFile, score);
            return;
        }
        if (current.Score <= score) return;
        current = new(caseFile, score);
    }

    public bool TargetReached() =>
        deliberationTarget.HasValue && current?.Score <= deliberationTarget.Value;

    public CaseFile GetCaseFile() => current!.CaseFile;
}
