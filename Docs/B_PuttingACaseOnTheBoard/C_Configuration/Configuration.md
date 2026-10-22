# Configuration
|[◀][RunningACheckr]|[△][PuttingACaseOnTheBoard]|[▶][TestFrameworkIntegration]|
|---|---|---|
  
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about.

All configuration options live in the `QuickCheckr.Protocol` namespace.  

**Default Configuration:**  
```csharp
{
    FileAs = string.Empty,
    Custodian = Custodian.Default,
    ShrinkMode = ShrinkMode.Default,
    ReportMode = ReportMode.Default,
    WarningLevel = WarningLevel.Important,
    MaxStepsPerReducer = 1024
}
```
## The Custodian
`TheCustodian` is in charge of storing and retrieving QuickCheckr artifacts.  
As we saw in 'Filing Evidence', QuickCheckr stores reports and failing seeds on disk.  
This is the default behaviour, but by configuring the `Custodian` we can change this behaviour.

Currently only one extra `Custodian` is provided: `Custodian.Local()`.  
This one stores everything in a '.quickcheckr' directory in the same directory as the test file that created it.
  
### Custom Custodians
You can also supply your own implementation:  
```csharp
namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian(HashSet<VaultEntry> StartingEvidence) : ICustodian
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

    public string RemoveFileCalledWith { get; private set; } = string.Empty;
    public void RemoveEvidence(string name, int seed)
    {
        var path = EvidenceFilename(name, seed);
        RemoveFileCalledWith = path;
        Records.Remove(path);
    }

    public HashSet<VaultEntry> Entries { get; private set; } = StartingEvidence;
    public void StoreVaultEntries(string name, IEnumerable<VaultEntry> entries)
    {
        Entries = [.. entries];
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string name)
        => Entries;
}
```
This one stores evidence and reports in memory, and is used in the tests to avoid touching the filesystem.

Obviously, this is the hook provided for when you would like to produce your own artifacts in a CI build.  
## ReportMode
You can use this to customize report formatting somewhat.  
I personally almost always disable the *Labels* ('Input:', 'Pool:', etc...), but your milage may vary.  

It is also useful to toggle traces on or off, for instance.  
```csharp
private static CheckrConfig DisableTraces(CheckrConfig config) =>
    config with { ReportMode = config.ReportMode & ~ReportMode.Trace };
```
A full reference to the available options can be found [here][A_ReportModeReference].  
## ShrinkMode
Controls which shrinking strategies are active.  

This is an option you will likely never need.  
It's main use is to investigate what happened during shrinking.  
As such, disabling shrinking entirely is probably the most useful step.  
```csharp
private static CheckrConfig DisableShrinking(CheckrConfig config) =>
    config with { ShrinkMode = ShrinkMode.None };
```
A full reference to the available options can be found [here][A_ShrinkModeReference].  
## Hint
If you do not like the defaults,
it is advised to simply wrap them into your own `CheckrOf` extension method(s):  
  
**Auto File:**  
```csharp
public static void FileThis(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr
        .Configure(a => a with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" })
        .Run(50.Runs(), 20.ExecutionsPerRun());
```
**Investigating:**  
```csharp
public static void Investigate(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr
        .Configure(cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" })
        .Conduct(100.Investigations(), 10.Runs(), 20.ExecutionsPerRun());
```
You get the idea.  

[A_ReportModeReference]: J_ReportMode/Reference/A_ReportModeReference.md

[A_ShrinkModeReference]: N_ShrinkMode/Reference/A_ShrinkModeReference.md

[PuttingACaseOnTheBoard]: ../PuttingACaseOnTheBoard.md

[RunningACheckr]: ../B_RunningACheckr/RunningACheckr.md

[TestFrameworkIntegration]: ../F_TestFrameworkIntegration/TestFrameworkIntegration.md
