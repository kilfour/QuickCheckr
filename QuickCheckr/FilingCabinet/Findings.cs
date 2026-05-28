using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.FilingCabinet;

public class Findings : IRecord
{
    private readonly List<CaseFile> caseFiles = [];
    public IReadOnlyList<CaseFile> CaseFiles => caseFiles;
    public int NumberOfRuns => caseFiles.Count;
    public List<PassedExpectation> PassedExpectationDepositions =>
        [.. caseFiles
            .SelectMany(cf => cf.PassedExpectations)
            .GroupBy(ps => ps.Label)
            .Select(g => new PassedExpectation(g.Key, g.Sum(ps => ps.TimesPassed)))];

    public Findings Add(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
            caseFiles.Add(caseFile);
        return this;
    }
}
