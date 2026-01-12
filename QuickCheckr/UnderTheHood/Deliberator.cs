using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.UnderTheHood;

public class Deliberator(Func<ICaseFileSummary, int>? scoringFunction, int? deliberationTarget)
{
    private readonly Func<ICaseFileSummary, int>? scoringFunction = scoringFunction;
    private readonly int? deliberationTarget = deliberationTarget;
    private DeliberatedCaseFile? current = null;

    public bool HasCaseFile => current is not null;

    public void Check(CaseFile caseFile)
    {
        if (scoringFunction is null)
        {
            current = new(caseFile, 0);
            return;
        }
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