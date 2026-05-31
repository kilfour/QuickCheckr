using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.FilingCabinet;

public class Findings : IRecord
{
    public CaseFile? FalsifyingCaseFile { get; private set; } = null;
    public Findings AddFalsifyingCaseFile(CaseFile falsifyingCaseFile)
    {
        FalsifyingCaseFile = falsifyingCaseFile;
        return this;
    }

    private readonly List<CaseFile> caseFiles = [];
    public IReadOnlyList<CaseFile> CaseFiles => caseFiles;
    public Findings Add(params CaseFile[] newCaseFiles)
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
