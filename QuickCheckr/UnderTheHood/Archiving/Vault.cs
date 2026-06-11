using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol.Custodians;

namespace QuickCheckr.UnderTheHood.Archiving;

public class VaultNotConfiguredException : Exception;
public static class Vault
{
    public static EvidenceVault From(CheckrRunnerConfig config)
        => string.IsNullOrWhiteSpace(config.FileAs) ? throw new VaultNotConfiguredException() : new EvidenceVault(config);
}

public class EvidenceVault(CheckrRunnerConfig config)
{

    private HashSet<VaultEntry> entries = [];
    private readonly ICustodian custodian = config.Custodian;



    public IEnumerable<CaseFile> Check(CheckrOf<Case> check, CheckrRunnerConfig config, bool closeResolved)
    {
        entries = custodian.RetrieveVaultEntries(config.FileAs);
        var caseFiles = new List<CaseFile>();
        foreach (var entry in entries)
        {
            var caseFile = new CheckrRunner(check, config with { Seed = entry.Seed }, entry.ExecutionsPerRun, Microtome.Empty).Run();
            caseFiles.Add(caseFile);
            if (caseFile.HasEvidence)
            {
                if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    custodian.FileEvidence(config.FileAs, entry.Seed, caseFile, config.Clerk.File);
                }
            }
            else
            {
                if (closeResolved)
                {
                    Remove(entry);
                    custodian.RemoveEvidence(config.FileAs, entry.Seed);
                }
                else
                {
                    custodian.FileEvidence(config.FileAs, entry.Seed, caseFile, config.Clerk.File);
                }
            }
        }
        custodian.StoreVaultEntries(config.FileAs, entries);
        return caseFiles;
    }

    public void AddSeed(int seed, int numberOfExecutions)
    {
        Add(new(seed, numberOfExecutions));
    }

    private void Add(VaultEntry vaultEntry)
    {
        entries.Add(vaultEntry);
        custodian.StoreVaultEntries(config.FileAs, entries);
    }

    private void Remove(VaultEntry vaultEntry)
    {
        entries.Remove(vaultEntry);
        custodian.StoreVaultEntries(config.FileAs, entries);
    }
}