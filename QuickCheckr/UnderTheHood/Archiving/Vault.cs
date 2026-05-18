using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol.Custodians;

namespace QuickCheckr.UnderTheHood.Archiving;

public class Vault
{
    public virtual IEnumerable<CaseFile> Check(CheckrOf<Case> check, CheckrRunnerConfig config, bool closeResolved) => [];
    public virtual bool Replay() { return false; }
    public virtual void AddSeed(int seed, int numberOfExecutions) { }

    public static Vault From(CheckrRunnerConfig config)
        => string.IsNullOrWhiteSpace(config.FileAs) ? new Vault() : new EvidenceVault(config);
}

public class EvidenceVault : Vault
{
    private readonly string evidencePath;
    private readonly string logPath;
    private readonly HashSet<VaultEntry> entries;
    private readonly ICustodian custodian;

    public EvidenceVault(CheckrRunnerConfig config)
    {
        evidencePath = GetVaultPath(config.FileAs, "evidence.qv");
        logPath = GetVaultPath(config.FileAs, "evidence-log.ql");
        custodian = config.Custodian;
        entries = custodian.RetrieveVaultEntries(evidencePath);//GetVaultEntries(config);
    }

    private static string GetVaultPath(string fileAs, string filename)
        => Path.Combine(Constants.ReportFolder, fileAs, Constants.EvidenceFolder, filename);

    public override IEnumerable<CaseFile> Check(CheckrOf<Case> check, CheckrRunnerConfig config, bool closeResolved)
    {
        var caseFiles = new List<CaseFile>();
        foreach (var entry in entries)
        {
            var caseFile = new CheckrRunner(check, config with { Seed = entry.Seed }, entry.ExecutionsPerRun).Run();
            if (caseFile.HasEvidence)
            {
                caseFiles.Add(caseFile);
                custodian.LogVaultAction(logPath, $"Run failed for Seed: {entry.Seed}.");
                if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    custodian.FileRecord(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"), caseFile, config.StyleGuide);
                }
            }
            else
            {
                custodian.LogVaultAction(logPath, $"Run passed for Seed: {entry.Seed}.");
                if (closeResolved)
                {
                    Remove(entry);
                    custodian.RemoveRecord(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"));
                }
                else if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    custodian.FileRecord(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"), caseFile, config.StyleGuide);
                }
            }
        }
        custodian.StoreVaultEntries(evidencePath, entries);
        return caseFiles;
    }

    public override void AddSeed(int seed, int numberOfExecutions)
    {
        Add(new(seed, numberOfExecutions));
    }

    private void Add(VaultEntry vaultEntry)
    {
        if (entries.Add(vaultEntry))
        {
            custodian.LogVaultAction(logPath, $"Added Seed: {vaultEntry.Seed}.");
        }
        custodian.StoreVaultEntries(evidencePath, entries);
    }

    private void Remove(VaultEntry vaultEntry)
    {
        if (entries.Remove(vaultEntry))
        {
            custodian.LogVaultAction(logPath, $"Removed Seed: {vaultEntry.Seed}.");
        }
    }
}