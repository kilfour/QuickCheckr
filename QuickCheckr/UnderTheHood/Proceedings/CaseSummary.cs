namespace QuickCheckr.UnderTheHood.Proceedings;

public class CaseSummary(int numberOfRuns) : CaseFile(null!)
{
    public int NumberOfRuns { get; } = numberOfRuns;

    public static CaseSummary FromCaseFiles(int numberOfRuns, IEnumerable<CaseFile> caseFiles)
        => new(numberOfRuns)
        {
            PassedExpectationDepositions = [..
                caseFiles
                    .SelectMany(cf => cf.PassedExpectationDepositions)
                    .GroupBy(ps => ps.Label)
                    .Select(g => new PassedExpectationDeposition(g.Key, g.Sum(ps => ps.TimesPassed)))]
        };
}
