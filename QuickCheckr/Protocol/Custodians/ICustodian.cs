using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse;

namespace QuickCheckr.Protocol.Custodians;

public interface ICustodian
{
    public void FileRecord(string name, IRecord record, Func<IRecord, Flow<Flow>> clerk);
    public void FileEvidence(string name, int seed, IRecord record, Func<IRecord, Flow<Flow>> clerk);
    public void RemoveEvidence(string name, int seed);
    public void StoreVaultEntries(string name, IEnumerable<VaultEntry> entries);
    public HashSet<VaultEntry> RetrieveVaultEntries(string name);
}

