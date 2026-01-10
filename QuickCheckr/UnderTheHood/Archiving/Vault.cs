using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Archiving;

public class Vault
{
    public virtual IEnumerable<CaseFile> Check(CheckrOf<Case> check, CheckrRunnerConfig config, bool closeResolved) => [];
    public virtual bool Replay() { return false; }
    public virtual void AddSeed(int seed, int numberOfExecutions, string failure) { }

    public static Vault From(CheckrRunnerConfig config)
        => string.IsNullOrWhiteSpace(config.FileAs) ? new Vault() : new EvidenceVault(config);
}

public class EvidenceVault : Vault
{
    private readonly string path;
    private readonly string logPath;
    private readonly HashSet<VaultEntry> entries;

    private readonly IEvidenceRecorder evidenceRecorder;
    private readonly IEvidenceRecorder logRecorder;

    public EvidenceVault(CheckrRunnerConfig config)
    {
        path = GetVaultPath(config.FileAs, "evidence.qv");
        logPath = GetVaultPath(config.FileAs, "evidence-log.ql");
        entries = GetVaultEntries(config);
        evidenceRecorder = config.Custodian.Recorder(path);
        logRecorder = config.Custodian.Recorder(logPath);
    }

    private HashSet<VaultEntry> GetVaultEntries(CheckrRunnerConfig config)
    {
        return config.Custodian.Retriever(path)
            .Select(ParseVaultEntry)
            .Where(entry => entry != null)
            .ToHashSet()!;
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
                logRecorder.Append($"Run failed for Seed: {entry.Seed}.");
                if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    config.Custodian.Recorder(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"))
                        .Append(TheClerk.Transcribes(caseFile));
                }
            }
            else
            {
                logRecorder.Append($"Run passed for Seed: {entry.Seed}.");
                if (closeResolved)
                {
                    Remove(entry);
                    config.Custodian.Recorder(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"))
                        .Clear();
                }

                else if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    config.Custodian.Recorder(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr"))
                        .Append(TheClerk.Transcribes(caseFile));
                }
            }
        }
        Save();
        return caseFiles;
    }

    public override void AddSeed(int seed, int numberOfExecutions, string failure)
    {
        Add(new(seed, numberOfExecutions, failure));
    }

    private void Add(VaultEntry vaultEntry)
    {
        if (entries.Add(vaultEntry))
        {
            logRecorder.Append($"Added Seed: {vaultEntry.Seed}.");
        }
        Save();
    }

    private static VaultEntry ParseVaultEntry(string line)
    {
        var parts = line.Split(',');
        return parts.Length == 3 &&
            int.TryParse(parts[0], out var seed) &&
            int.TryParse(parts[1], out var executions)
            ? new VaultEntry(seed, executions, parts[2])
            : null!;
    }

    private void Remove(VaultEntry vaultEntry)
    {
        if (entries.Remove(vaultEntry))
        {
            logRecorder.Append($"Removed Seed: {vaultEntry.Seed}.");
        }
    }

    private void Save()
    {
        foreach (var entry in entries.Select(a => $"{a.Seed},{a.ExecutionsPerRun},{a.Failure}"))
        {
            evidenceRecorder.Append(entry);
        }
        if (entries.Count == 0)
            evidenceRecorder.Clear();
    }
}