namespace QuickCheckr.UnderTheHood.Proceedings;

public class CaseSummary(int numberOfRuns) : CaseFile(null!)
{
    public int NumberOfRuns { get; } = numberOfRuns;

    public static CaseSummary FromCaseFiles(int numberOfRuns, IEnumerable<CaseFile> caseFiles)
        => new(numberOfRuns)
        {
            PassedSpecDepositions = [..
                caseFiles
                    .SelectMany(cf => cf.PassedSpecDepositions)
                    .GroupBy(ps => ps.Label)
                    .Select(g => new PassedSpecDeposition(g.Key, g.Sum(ps => ps.TimesPassed)))]
        };
}
