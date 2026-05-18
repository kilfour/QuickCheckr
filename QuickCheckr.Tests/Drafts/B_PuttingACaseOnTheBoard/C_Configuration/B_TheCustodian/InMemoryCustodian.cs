using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian(HashSet<VaultEntry> StartingEvidence) : ICustodian
{
    public Dictionary<string, IRecord> Records { get; private set; } = [];

    public void FileRecord(string path, IRecord record, Func<IRecord, Flow<Flow>> styleGuide)
        => Records[path] = record;

    public string RemoveFileCalledWith { get; private set; } = string.Empty;
    public void RemoveRecord(string path)
    {
        RemoveFileCalledWith = path;
        Records.Remove(path);
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