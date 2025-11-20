using QuickCheckr.UnderTheHood.Docketing;
using QuickFuzzr.UnderTheHood;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrState(Signal<IFiling>? signal, int? seed = null)
{
    public State FuzzrState { get; init; } = seed is null ? new() : new(seed.Value);
    public LongTermMemory LongTermMemory { get; } = new();
    public ShortTermMemory ShortTermMemory { get; } = new();
    public EvidenceRecorder EvidenceRecorder { get; init; } = new();
    public Docket Docket { get; init; } = new(signal);
}
