using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.FilingCabinet;

public class Inquiry : IRecord
{
    private readonly List<CaseFile> caseFiles = [];
    public IReadOnlyList<CaseFile> CaseFiles => caseFiles;

    private readonly List<CaseFile> distinctFailures = [];
    public IReadOnlyList<CaseFile> DistinctFailures => distinctFailures;

    public int NumberOfRuns => caseFiles.Count;
    public int FailureCount => caseFiles.Count(a => a.HasEvidence);
    public int DistinctFailureCount => distinctFailures.Count;

    public List<PassedExpectationDeposition> PassedExpectationDepositions =>
        [.. caseFiles
            .SelectMany(cf => cf.PassedExpectationDepositions)
            .GroupBy(ps => ps.Label)
            .Select(g => new PassedExpectationDeposition(g.Key, g.Sum(ps => ps.TimesPassed)))];

    public Inquiry Add(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
            caseFiles.Add(caseFile);
        return this;
    }

    public Inquiry AddDistinctFailure(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
        {
            distinctFailures.Add(caseFile);
        }
        return this;
    }
}