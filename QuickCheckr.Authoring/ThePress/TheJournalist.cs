

using System.Diagnostics;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Authoring.ThePress;


public class Journalist : ICustodian
{
    public Dictionary<string, IRecord> Records { get; private set; } = [];

    public void FileRecord(string name, IRecord record, Func<IRecord, Flow<Flow>> styleGuide)
        => Records[name] = record;

    public void FileEvidence(string name, int seed, IRecord record, Func<IRecord, Flow<Flow>> clerk)
        => Records[EvidenceFilename(name, seed)] = record;

    private static string EvidenceFilename(string name, int seed)
    {
        return $"{name}_{seed}";
    }

    public void RemoveEvidence(string name, int seed)
    {
        var path = EvidenceFilename(name, seed);
        RemoveFileCalledWith = path;
        Records.Remove(path);
    }

    public string RemoveFileCalledWith { get; private set; } = string.Empty;
    public void RemoveRecord(string path)
    {
        RemoveFileCalledWith = path;
        Records.Remove(path);
    }

    public HashSet<VaultEntry> Entries { get; private set; } = [];
    public void StoreVaultEntries(string name, IEnumerable<VaultEntry> entries)
    {
        Entries = [.. entries];
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string name)
        => Entries;
}

public static class TheJournalist
{
    [StackTraceHidden]
    public static Article Unearths(IRecord record) => new(record);

    [StackTraceHidden]
    public static Article Exposes(Action run)
    {
        try
        {
            run();
        }
        catch (FalsifiableException ex)
        {
            return Unearths(ex.TheCaseFile);
        }
        throw new InvalidOperationException("Expected FalsifiableException, but none was thrown.");
    }

    [StackTraceHidden]
    public static Article Investigates(Func<IRecord> run)
    {
        try
        {
            return Unearths(run());
        }
        catch (FalsifiableException ex)
        {
            return Unearths(ex.TheCaseFile);
        }
    }

    public static void Transcribes(Article article) =>
        Signal.From<Article>(TheEditor.ProofReads)
            .SetArtery(FileLog.Write())
            .Pulse(article);
}
