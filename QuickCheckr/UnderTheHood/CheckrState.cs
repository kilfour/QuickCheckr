using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories;
using QuickFuzzr.UnderTheHood;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrState(Signal<IFiling>? signal, int? seed = null)
{
    public State FuzzrState { get; init; } = seed is null ? new() : new(seed.Value);
    public Memory Memory { get; init; } = new();
    public EvidenceRecorder EvidenceRecorder { get; init; } = new();
    public Docket Docket { get; init; } = new(signal);

    public int StartReplay()
    {
        EvidenceRecorder.StartReplay();
        return Memory.Reset();
    }

    private bool runStopped = false;
    public bool RunStopped => runStopped;
    public void StopRun()
    {
        runStopped = true;
    }
}