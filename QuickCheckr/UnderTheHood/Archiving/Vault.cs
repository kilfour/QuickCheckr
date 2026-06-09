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
    const string EvidenceFolder = "vault";

    private readonly string evidencePath = GetVaultPath(config.FileAs, "evidence.qv");
    private HashSet<VaultEntry> entries = [];
    private readonly ICustodian custodian = config.Custodian;

    public static string GetVaultPath(string fileAs, string filename)
        => Path.Combine(ReportPath.MainFolder, fileAs, EvidenceFolder, filename);

    public IEnumerable<CaseFile> Check(CheckrOf<Case> check, CheckrRunnerConfig config, bool closeResolved)
    {
        entries = custodian.RetrieveVaultEntries(evidencePath);
        var caseFiles = new List<CaseFile>();
        foreach (var entry in entries)
        {
            var caseFile = new CheckrRunner(check, config with { Seed = entry.Seed }, entry.ExecutionsPerRun, []).Run();
            caseFiles.Add(caseFile);
            if (caseFile.HasEvidence)
            {
                if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    custodian.FileRecord(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"), caseFile, config.Clerk.File);
                }
            }
            else
            {
                if (closeResolved)
                {
                    Remove(entry);
                    custodian.RemoveRecord(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"));
                }
                else if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    custodian.FileRecord(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"), caseFile, config.Clerk.File);
                }
            }
        }
        custodian.StoreVaultEntries(evidencePath, entries);
        return caseFiles;
    }

    public void AddSeed(int seed, int numberOfExecutions)
    {
        Add(new(seed, numberOfExecutions));
    }

    private void Add(VaultEntry vaultEntry)
    {
        entries.Add(vaultEntry);
        custodian.StoreVaultEntries(evidencePath, entries);
    }

    private void Remove(VaultEntry vaultEntry)
    {
        entries.Remove(vaultEntry);
        custodian.StoreVaultEntries(evidencePath, entries);
    }
}