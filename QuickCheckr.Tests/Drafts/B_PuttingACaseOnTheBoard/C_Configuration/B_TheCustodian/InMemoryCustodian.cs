using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian(HashSet<VaultEntry> StartingEvidence) : ICustodian
{
    public Dictionary<string, IRecord> Records { get; private set; } = [];

    public void FileRecord(string name, IRecord record, Func<IRecord, Flow<Flow>> styleGuide)
        => Records[name] = record;

    public void FileEvidence(string name, int seed, IRecord record, Func<IRecord, Flow<Flow>> clerk)
        => Records[EvidenceFilename(name, seed)] = record;

    private static string EvidenceFilename(string name, int seed)
    {
        return $"{name}_{seed}";
    }

    public string RemoveFileCalledWith { get; private set; } = string.Empty;
    public void RemoveEvidence(string name, int seed)
    {
        var path = EvidenceFilename(name, seed);
        RemoveFileCalledWith = path;
        Records.Remove(path);
    }

    public HashSet<VaultEntry> Entries { get; private set; } = StartingEvidence;
    public void StoreVaultEntries(string name, IEnumerable<VaultEntry> entries)
    {
        Entries = [.. entries];
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string name)
        => Entries;
}