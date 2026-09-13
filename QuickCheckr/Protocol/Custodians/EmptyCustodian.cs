using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse;

namespace QuickCheckr.Protocol.Custodians;

public sealed class EmptyCustodian : ICustodian
{
    public void FileEvidence(string name, int seed, IRecord record, Func<IRecord, Flow<Flow>> clerk) { }
    public void FileRecord(string name, IRecord record, Func<IRecord, Flow<Flow>> clerk) { }
    public void RemoveEvidence(string name, int seed) { }
    public HashSet<VaultEntry> RetrieveVaultEntries(string name) => [];
    public void StoreVaultEntries(string name, IEnumerable<VaultEntry> entries) { }
}
