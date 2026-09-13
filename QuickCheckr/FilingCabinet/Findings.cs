using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.FilingCabinet;

public sealed class Findings : IRecord
{
    internal Findings() { }

    public CaseFile? FalsifyingCaseFile { get; private set; } = null;
    internal Findings AddFalsifyingCaseFile(CaseFile falsifyingCaseFile)
    {
        FalsifyingCaseFile = falsifyingCaseFile;
        return this;
    }

    private readonly List<CaseFile> caseFiles = [];
    public IReadOnlyList<CaseFile> CaseFiles => caseFiles;
    internal Findings Add(params CaseFile[] newCaseFiles)
    {
        foreach (var caseFile in newCaseFiles)
            caseFiles.Add(caseFile);
        return this;
    }

    public int NumberOfRuns => caseFiles.Count;
    public List<PassedExpectation> PassedExpectations =>
        [.. caseFiles
            .SelectMany(cf => cf.PassedExpectations)
            .GroupBy(ps => ps.Label)
            .Select(g => new PassedExpectation(g.Key, g.Sum(ps => ps.TimesPassed)))];
}
