using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Protocol;

public class Custodian
{
    public required Func<string, IEnumerable<string>> Retriever { get; init; }
    public required Func<string, IArtery> Recorder { get; init; }

    public static Custodian Default
        => new() { Retriever = GetVaultStrings, Recorder = a => FileLog.Write(a) };

    private static string[] GetVaultStrings(string path)
        => File.Exists(path) ? File.ReadAllLines(path) : [];
}
