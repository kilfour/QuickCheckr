using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Protocol.Custodians;

public abstract class FileCustodian : ICustodian
{
    const string MainFolder = ".quickcheckr";
    const string EvidenceFolder = "vault";

    private static string GetReportFilename(string name)
        => Path.Combine(MainFolder, name, "report.qr");

    private static string GetVaultPath(string name, string filename)
        => Path.Combine(MainFolder, name, EvidenceFolder, filename);

    private static string GetEvidenceFilename(string name, int seed)
        => GetVaultPath(name, $"report_{seed}.qr");

    private static string GetVaultEntriesFilename(string name)
        => GetVaultPath(name, "evidence.qv");

    public void FileRecord(string name, IRecord record, Func<IRecord, Flow<Flow>> styleGuide)
        => GetFileLogArtery(GetReportFilename(name))
            .ClearFile()
            .Absorb(TheClerk.Transcribes(record, styleGuide));

    public void FileEvidence(string name, int seed, IRecord record, Func<IRecord, Flow<Flow>> styleGuide)
        => GetFileLogArtery(GetEvidenceFilename(name, seed))
            .ClearFile()
            .Absorb(TheClerk.Transcribes(record, styleGuide));

    public void RemoveEvidence(string name, int seed) => RemoveFile(GetEvidenceFilename(name, seed));

    public void StoreVaultEntries(string name, IEnumerable<VaultEntry> entries)
    {
        var path = GetVaultEntriesFilename(name);
        if (entries.Any())
            GetFileLogArtery(path).ClearFile().Absorb([.. entries.Select(a => $"{a.Seed},{a.ExecutionsPerRun}")]);
        else
            RemoveFile(path);
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string name)
    {
        var realPath = GetRealPath(GetVaultEntriesFilename(name));
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
