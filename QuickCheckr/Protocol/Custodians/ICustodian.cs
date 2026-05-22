using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse;

namespace QuickCheckr.Protocol.Custodians;

public interface ICustodian
{
    public void FileRecord(string path, IRecord record, Func<IRecord, Flow<Flow>> clerk);
    public void RemoveRecord(string path);
    public void StoreVaultEntries(string path, IEnumerable<VaultEntry> entries);
    public HashSet<VaultEntry> RetrieveVaultEntries(string path);
}

