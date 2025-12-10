using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse.Arteries;


namespace QuickCheckr.Tests.Tools;

public class SpyRecorder() : Collector<string>, IEvidenceRecorder
{
    public bool Cleared { get; private set; }
    public void Append(string evidence) => Absorb(evidence);

    public void Clear() => Cleared = true;
}
