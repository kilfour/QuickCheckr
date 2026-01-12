using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse.Arteries;

namespace QuickCheckr.Protocol.Custodians;

public abstract class FileCustodian : ICustodian
{
    public void StoreCaseFile(string path, CaseFile caseFile)
        => GetFileLogArtery(path).ClearFile().Absorb(TheClerk.Transcribes(caseFile));

    public void RemoveCaseFile(string path) => RemoveFile(path);

    public void LogVaultAction(string path, string line)
        => GetFileLogArtery(path).Absorb(line);

    public void StoreVaultEntries(string path, IEnumerable<VaultEntry> entries)
    {
        if (entries.Any())
            GetFileLogArtery(path).Absorb([.. entries.Select(a => $"{a.Seed},{a.ExecutionsPerRun}")]);
        else
            RemoveFile(path);
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string path)
    {
        var realPath = GetRealPath(path);
        return File.Exists(realPath) ? [.. File.ReadAllLines(realPath).Select(ParseVaultEntry)] : [];
    }

    protected abstract string GetRealPath(string path);
    protected abstract FileLogArtery GetFileLogArtery(string path);

    private static VaultEntry ParseVaultEntry(string line)
    {
        var parts = line.Split(',');
        return parts.Length == 2 &&
            int.TryParse(parts[0], out var seed) &&
            int.TryParse(parts[1], out var executions)
            ? new VaultEntry(seed, executions)
            : null!;
    }
    private void RemoveFile(string path)
    {
        var realPath = GetRealPath(path);
        if (File.Exists(realPath))
            File.Delete(realPath);
    }
}
