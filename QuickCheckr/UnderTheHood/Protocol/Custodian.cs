using QuickPulse.Arteries;
using QuickPulse.Instruments;

namespace QuickCheckr.UnderTheHood.Protocol;

public class Custodian
{
    public required Func<string, IEnumerable<string>> Retriever { get; init; }
    public required Func<string, IArtery> Recorder { get; init; }

    public static Custodian Default
        => new() { Retriever = GetVaultStrings, Recorder = a => FileLog.Write(a) };

    private static string[] GetVaultStrings(string path)
    {
        var solutionPath = Path.Combine(SolutionLocator.FindSolutionRoot()!, path);
        return File.Exists(solutionPath) ? File.ReadAllLines(solutionPath) : [];
    }
}

