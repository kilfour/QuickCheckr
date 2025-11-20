using QuickCheckr.UnderTheHood.Docketing;
using QuickFuzzr.UnderTheHood;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrState(Signal<IFiling>? signal)
{
    // Used by :
    // - Checkr.Input
    public State FuzzrState { get; init; } = new();
    // Used by :
    // - Trackr.Stashed
    public LongTermMemory LongTermMemory { get; } = new();
    // Used by :
    // - Checkr.Input
    public ShortTermMemory ShortTermMemory { get; } = new();
    // Used by :
    // - Checkr.Act
    // - Checkr.Spec
    public EvidenceRecorder EvidenceRecorder { get; init; } = new();

    // Diagnostics Hook
    // Used by :
    // - CheckrRunner
    // - Checkr.Input
    // - Checkr.Act
    // - Checkr.Spec
    public Docket Docket { get; init; } = new(signal);
}
