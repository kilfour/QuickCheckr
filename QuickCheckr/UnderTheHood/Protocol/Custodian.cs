using QuickPulse.Arteries;
using QuickPulse.Instruments;

namespace QuickCheckr.UnderTheHood.Protocol;

public class Custodian
{
    public required Func<string, IEnumerable<string>> Retriever { get; init; }
    public required Func<string, IArtery> Recorder { get; init; }

    public static Custodian Default
        => new() { Retriever = GetVaultStrings, Recorder = a => FileLog.Write(a) };//new FileLogDeleteWhenEmpty(a) };

    private static string[] GetVaultStrings(string path)
    {
        var solutionPath = Path.Combine(SolutionLocator.FindSolutionRoot()!, path);
        return File.Exists(solutionPath) ? File.ReadAllLines(solutionPath) : [];
    }
}

public class FileLogDeleteWhenEmpty : IArtery
{
    public string FilePath { get; init; }

    public void Absorb(params object[] data)
    {
        // if (data == null || data.Length == 0)
        //     File.Delete(FilePath);
        // else
        File.AppendAllLines(FilePath, data.Select(a => a.ToString()!));
    }

    public FileLogDeleteWhenEmpty(string? maybeFileName = null, bool relativeToSolution = true)
    {
        FilePath = GetFilePath(maybeFileName, relativeToSolution);
        File.Delete(FilePath);
        EnsureDirectoryExists();
    }

    private static string GetFilePath(string? maybeFileName, bool relativeToSolution)
    {
        var fileName = maybeFileName ?? GetDefaultFileName();
        if (relativeToSolution)
            fileName = RelateToSolution(fileName);
        return Path.GetFullPath(fileName);
    }

    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    private static string GetDefaultFileName()
        => Path.Combine(".quickpulse", $"quick-pulse-{GetUniqueSuffix()}.log");

    private static string GetUniqueSuffix()
        => $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}_{Path.GetRandomFileName().Replace(".", "")}";

    private static string RelateToSolution(string fileName)
    {
        var root = SolutionLocator.FindSolutionRoot();
        if (root == null)
            ComputerSays.No("Cannot find solution root.");
        return Path.Combine(root!, fileName);
    }
}

