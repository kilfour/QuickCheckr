using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse;

namespace QuickCheckr.Protocol.Custodians;

public interface ICustodian
{
    public void StoreCaseFile(string path, CaseFile caseFile, Flow<CaseFile> styleGuide);
    public void RemoveCaseFile(string path);
    public void StoreVaultEntries(string path, IEnumerable<VaultEntry> entries);
    public HashSet<VaultEntry> RetrieveVaultEntries(string path);
    public void LogVaultAction(string path, string line);
}

