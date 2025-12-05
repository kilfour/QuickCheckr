using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Protocol;
using QuickCheckr.UnderTheHood.Runners;
using QuickPulse;
using QuickPulse.Arteries;
using QuickPulse.Instruments;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Archiving;

public class Vault
{
    public virtual void Check(CheckrOf<Case> check, CheckrRunnerConfig config, bool closeResolved) { }
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

    private readonly Signal<string> evidenceSignal;
    private readonly Signal<string> logSignal;

    public EvidenceVault(CheckrRunnerConfig config)
    {
        path = GetVaultPath(config.FileAs, "evidence.qv");
        logPath = GetVaultPath(config.FileAs, "evidence-log.ql");
        entries = GetVaultEntries(config);
        evidenceSignal = Signal.From<string>(a => Pulse.Trace(a))
            .SetArtery(config.Custodian.Recorder(path));
        logSignal = Signal.From<string>(a => Pulse.Trace(a))
            .SetArtery(config.Custodian.Recorder(logPath));
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

    public override void Check(CheckrOf<Case> check, CheckrRunnerConfig config, bool closeResolved)
    {
        foreach (var entry in entries)
        {
            config.SetSeed(entry.Seed);
            var caseFile = new CheckrRunner(check, config, entry.ExecutionsPerRun).Run();
            if (caseFile.HasVerdict())
            {
                if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    Signal.From<string>(a => Pulse.Trace(a))
                        .SetArtery(config.Custodian.Recorder(GetVaultPath(config.FileAs, $"report_{entry.Seed}.qr")))
                        .Pulse(TheClerk.Transcribes(caseFile));
                }
                logSignal.Pulse($"Run failed for Seed: {entry.Seed}.{Environment.NewLine}");
            }
            else
            {
                logSignal.Pulse($"Run passed for Seed: {entry.Seed}.{Environment.NewLine}");
                if (closeResolved)
                    Remove(entry);
            }
        }
        Save();
    }

    public override void AddSeed(int seed, int numberOfExecutions, string failure)
    {
        Add(new(seed, numberOfExecutions, failure));
    }

    private void Add(VaultEntry vaultEntry)
    {
        if (entries.Add(vaultEntry))
        {
            Save();
            logSignal.Pulse($"Added Seed: {vaultEntry.Seed}.{Environment.NewLine}");
        }

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
            SaveOrDelete();
            logSignal.Pulse($"Removed Seed: {vaultEntry.Seed}.{Environment.NewLine}");
        }
    }

    private void Save()
    {
        evidenceSignal.Pulse(entries.Select(a => $"{a.Seed},{a.ExecutionsPerRun},{a.Failure}"));
    }

    private void SaveOrDelete()
    {
        if (entries.Count == 0)
        {
            // if (File.Exists(path))
            //     File.Delete(path);
        }
        else
        {
            Save();
        }
    }
}