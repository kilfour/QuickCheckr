using QuickPulse.Arteries;
using QuickPulse.Instruments;

namespace QuickCheckr.UnderTheHood.Protocol;

public class Custodian
{
    public class FileRecorder(string filename) : FileLogArtery(filename), IEvidenceRecorder
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
}

