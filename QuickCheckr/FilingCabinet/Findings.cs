using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.FilingCabinet;

public class Findings : IRecord
{
    private readonly List<CaseFile> caseFiles = [];
    public IReadOnlyList<CaseFile> CaseFiles => caseFiles;
    public int NumberOfRuns => caseFiles.Count;
    public List<PassedExpectationDeposition> PassedExpectationDepositions =>
        [.. caseFiles
            .SelectMany(cf => cf.PassedExpectationDepositions)
            .GroupBy(ps => ps.Label)
            .Select(g => new PassedExpectationDeposition(g.Key, g.Sum(ps => ps.TimesPassed)))];

    public Findings Add(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
            caseFiles.Add(caseFile);
        return this;
    }
}
