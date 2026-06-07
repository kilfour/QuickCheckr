using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.FilingCabinet;

public class ColdCasesReview : IRecord
{
    private readonly List<CaseFile> caseFiles = [];
    public IReadOnlyList<CaseFile> CaseFiles => caseFiles;
    public ColdCasesReview Add(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
            caseFiles.Add(caseFile);
        return this;
    }

    public int NumberOfRuns => caseFiles.Count;
    public List<PassedExpectation> PassedExpectationDepositions =>
        [.. caseFiles
            .SelectMany(cf => cf.PassedExpectations)
            .GroupBy(ps => ps.Label)
            .Select(g => new PassedExpectation(g.Key, g.Sum(ps => ps.TimesPassed)))];
}