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

    public static Vault From(string fileAs)
        => string.IsNullOrWhiteSpace(fileAs) ? new Vault() : new EvidenceVault(fileAs);
}

public class EvidenceVault : Vault
{
    private readonly string path;
    private readonly string logPath;
    private readonly HashSet<VaultEntry> entries;

    public EvidenceVault(string name)
    {
        var root = SolutionLocator.FindSolutionRoot();
        root ??= AppContext.BaseDirectory;
        path = Path.Combine(root!, Constants.ReportFolder, name, Constants.EvidenceFolder, $"evidence.qv");
        logPath = Path.Combine(root!, Constants.ReportFolder, name, Constants.EvidenceFolder, $"evidence-log.ql");
        entries = GetVaultEntries();
    }

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
                    var filenameCf = Path.Combine(Constants.ReportFolder, config.FileAs, Constants.EvidenceFolder, $"report_{entry.Seed}.qr");
                    Signal.From<string>(a => Pulse.Trace(a))
                        .SetArtery(config.Reporter(filenameCf))
                        .Pulse(TheClerk.Transcribes(caseFile));
                }
                File.AppendAllText(logPath, $"Run failed for Seed: {entry.Seed}.{Environment.NewLine}");
            }
            else
            {
                File.AppendAllText(logPath, $"Run passed for Seed: {entry.Seed}.{Environment.NewLine}");
                if (closeResolved)
                    Remove(entry);
            }
        }
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
            File.AppendAllText(logPath, $"Added Seed: {vaultEntry.Seed}.{Environment.NewLine}");
        }

    }
    private HashSet<VaultEntry> GetVaultEntries()
    {
        return File.Exists(path) ? ReadEntriesFromFile() : [];
    }

    private HashSet<VaultEntry> ReadEntriesFromFile()
    {
        return File.ReadAllLines(path)
            .Select(ParseVaultEntry)
            .Where(entry => entry != null)
            .ToHashSet()!;
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
            File.AppendAllText(logPath, $"Removed Seed: {vaultEntry.Seed}.{Environment.NewLine}");
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllLines(path, entries.Select(a => $"{a.Seed},{a.ExecutionsPerRun},{a.Failure}"));
    }

    private void SaveOrDelete()
    {
        if (entries.Count == 0)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        else
        {
            Save();
        }
    }
}