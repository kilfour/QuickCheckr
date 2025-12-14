using System.Runtime.CompilerServices;
using QuickCheckr.UnderTheHood;
using QuickPulse.Arteries;
using QuickPulse.Instruments;

namespace QuickCheckr.Protocol;

public class Custodian
{
    public class FileRecorder(string filename, bool relativeToSolution = true) : FileLogArtery(filename, relativeToSolution), IEvidenceRecorder
    {
        public void Append(string evidence) => Absorb(evidence);
        public void Clear() => File.Delete(FilePath);
    }

    public required Func<string, IEnumerable<string>> Retriever { get; init; }
    public required Func<string, IEvidenceRecorder> Recorder { get; init; }

    public static Custodian Default
        => new()
        {
            Retriever = GetVaultStrings,
            Recorder = a =>
            {
                var recorder = new FileRecorder(a);
                recorder.ClearFile();
                return recorder;
            }
        };

    private static string[] GetVaultStrings(string path)
    {
        var solutionPath = Path.Combine(SolutionLocator.FindSolutionRoot()!, path);
        return File.Exists(solutionPath) ? File.ReadAllLines(solutionPath) : [];
    }

    public static Custodian Local([CallerFilePath] string callerPath = "")
        => new()
        {
            Retriever = a => GetLocalVaultStrings(Path.Combine(Path.GetDirectoryName(callerPath)!, a)),
            Recorder = a =>
            {
                var recorder = new FileRecorder(Path.Combine(Path.GetDirectoryName(callerPath)!, a), false);
                recorder.ClearFile();
                return recorder;
            }
        };

    private static string[] GetLocalVaultStrings(string path)
    {
        return File.Exists(path) ? File.ReadAllLines(path) : [];
    }
}

