using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse;
using QuickPulse.Arteries;
using QuickPulse.Instruments;

namespace QuickCheckr.Authoring.ThePress;


public class Journalist : ICustodian
{
    public static Article Publish(CheckrOf<Case> checkr, Action<ConfiguredCheckr> runCheckr)
    {
        var journalist = new Journalist();
        journalist.Investigate(checkr, runCheckr);
        return journalist.GetArticle();
    }

    public void Investigate(CheckrOf<Case> checkr, Action<ConfiguredCheckr> runCheckr)
    {
        try
        {
            runCheckr(checkr.Configure(a => a with { FileAs = "WhistleBlower", Custodian = this }));
        }
        catch (FalsifiableException) { }
    }

    public static Article Publish(ConfiguredCheckr configuredCheckr, Action<ConfiguredCheckr> runCheckr)
    {
        var journalist = new Journalist();
        journalist.Investigate(configuredCheckr, runCheckr);
        return journalist.GetArticle();
    }

    public void Investigate(ConfiguredCheckr configuredCheckr, Action<ConfiguredCheckr> runCheckr)
    {
        try
        {
            runCheckr(configuredCheckr.Configure(a => a with { FileAs = "WhistleBlower", Custodian = this }));
        }
        catch (FalsifiableException) { }
    }

    private IRecord? record;

    public IRecord GetRecord()
        => record is null
            ? ComputerSays.No<IRecord>("The journalist could not obtain any information.")
            : record;

    public Article GetArticle()
        => record is null
            ? ComputerSays.No<Article>("The journalist could not obtain any information.")
            : new(record);

    public Dictionary<string, IRecord> Evidence { get; private set; } = [];

    public void FileRecord(string name, IRecord record, Func<IRecord, Flow<Flow>> styleGuide)
        => this.record = record;

    public void FileEvidence(string name, int seed, IRecord record, Func<IRecord, Flow<Flow>> clerk)
        => Evidence[EvidenceFilename(name, seed)] = record;

    private static string EvidenceFilename(string name, int seed)
    {
        return $"{name}_{seed}";
    }

    public string RemoveFileCalledWith { get; private set; } = string.Empty;
    public void RemoveEvidence(string name, int seed)
    {
        var path = EvidenceFilename(name, seed);
        RemoveFileCalledWith = path;
        Evidence.Remove(path);
    }

    public HashSet<VaultEntry> Entries { get; private set; } = [];
    public void StoreVaultEntries(string name, IEnumerable<VaultEntry> entries)
    {
        Entries = [.. entries];
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string name)
        => Entries;

    public static void Transcribe(Article article, bool passedExpectationsContains) =>
        Signal.From<Article>(a => TheEditor.ProofReads(a, passedExpectationsContains))
            .SetArtery(FileLog.Write())
            .Pulse(article);
}
