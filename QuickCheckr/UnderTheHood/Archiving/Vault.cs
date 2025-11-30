using QuickCheckr.UnderTheHood.Runners;
using QuickPulse.Instruments;

namespace QuickCheckr.UnderTheHood.Archiving;

public class Vault
{
    public virtual void Check(CheckrOf<Case> check) { }
    public virtual bool Replay() { return false; }
    public virtual void AddSeed(int seed, int numberOfExecutions) { }
}

public class EvidenceVault : Vault
{
    private readonly string path;
    private readonly HashSet<VaultEntry> entries;

    public EvidenceVault(string name)
    {
        var root = SolutionLocator.FindSolutionRoot();
        root ??= AppContext.BaseDirectory;
        path = Path.Combine(root!, ".quickcheckr", name, $"evidence.qv");
        entries = GetVaultEntries();
    }

    public override void Check(CheckrOf<Case> check)
    {
        foreach (var entry in entries)
        {
            var state = new CheckrState(entry.ExecutionsPerRun, null!, entry.Seed);
            new InitialRun(state, check).Run(entry.ExecutionsPerRun);
            if (!state.EvidenceRecorder.Failed())
            {
                Remove(entry);
            }
        }
    }

    public override void AddSeed(int seed, int numberOfExecutions)
    {
        Add(new(seed, numberOfExecutions));
    }

    private void Add(VaultEntry vaultEntry)
    {
        if (entries.Add(vaultEntry))
            Save();
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
        return parts.Length == 2 &&
            int.TryParse(parts[0], out var seed) &&
            int.TryParse(parts[1], out var executions)
            ? new VaultEntry(seed, executions)
            : null!;
    }

    private void Remove(VaultEntry vaultEntry)
    {
        if (entries.Remove(vaultEntry))
            SaveOrDelete();
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllLines(path, entries.Select(a => $"{a.Seed},{a.ExecutionsPerRun}"));
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