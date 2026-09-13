using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.FilingCabinet;

public sealed class Inquiry : IRecord
{
    internal Inquiry() { }

    private readonly List<CaseFile> caseFiles = [];
    public IReadOnlyList<CaseFile> CaseFiles => caseFiles;

    private readonly List<CaseFile> distinctFailures = [];
    public IReadOnlyList<CaseFile> DistinctFailures => distinctFailures;

    public int NumberOfRuns => caseFiles.Count;
    public int FailureCount => caseFiles.Count(a => a.HasEvidence);
    public int DistinctFailureCount => distinctFailures.Count;

    public List<PassedExpectation> PassedExpectations =>
        [.. caseFiles
            .SelectMany(cf => cf.PassedExpectations)
            .GroupBy(ps => ps.Label)
            .Select(g => new PassedExpectation(g.Key, g.Sum(ps => ps.TimesPassed)))];

    internal Inquiry Add(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
            caseFiles.Add(caseFile);
        return this;
    }

    internal Inquiry AddDistinctFailure(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
        {
            distinctFailures.Add(caseFile);
        }
        return this;
    }

    public bool MaxStoredCaseFilesReached { get; private set; } = false;
    internal bool Stop() => MaxStoredCaseFilesReached = true;
}
