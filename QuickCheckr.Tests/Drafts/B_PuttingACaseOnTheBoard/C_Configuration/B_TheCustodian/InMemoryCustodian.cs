using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian(HashSet<VaultEntry> StartingEvidence) : ICustodian
{
    public Dictionary<string, CaseFile> CaseFiles { get; private set; } = [];

    public void StoreCaseFile(string path, CaseFile caseFile, Func<CaseFile, Flow<Flow>> styleGuide)
        => CaseFiles[path] = caseFile;

    public string RemoveFileCalledWith { get; private set; } = string.Empty;
    public void RemoveCaseFile(string path)
    {
        RemoveFileCalledWith = path;
        CaseFiles.Remove(path);
    }

    public string EvidenceFileName { get; private set; } = string.Empty;
    public HashSet<VaultEntry> Entries { get; private set; } = StartingEvidence;
    public void StoreVaultEntries(string path, IEnumerable<VaultEntry> entries)
    {
        EvidenceFileName = path;
        Entries = [.. entries];
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string path)
        => Entries;

    public string LogFileName { get; private set; } = string.Empty;
    public List<string> Log = [];
    public void LogVaultAction(string path, string line)
    {
        LogFileName = path;
        Log.Add(line);
    }
}