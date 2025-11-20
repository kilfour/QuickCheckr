using QuickCheckr.UnderTheHood.Docketing;
using QuickFuzzr.UnderTheHood;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrState(Signal<IFiling>? signal, int? seed = null)
{
    // Used by :
    // - Checkr.Input
    public State FuzzrState { get; init; } = seed is null ? new() : new(seed.Value);
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
