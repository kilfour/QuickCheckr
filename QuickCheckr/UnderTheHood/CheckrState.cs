using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories;
using QuickFuzzr.UnderTheHood;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrState(Signal<IFiling>? signal, int? seed = null)
{
    public State FuzzrState { get; init; } = seed is null ? new() : new(seed.Value);
    public LongTermMemory LongTermMemory { get; } = new();
    public ShortTermMemory ShortTermMemories { get; } = new();
    public EvidenceRecorder EvidenceRecorder { get; init; } = new();
    public Docket Docket { get; init; } = new(signal);
}


// public Replay Replay { get; private set; } = Replay.Empty;
// public void StartReplay() => Replay = new Replay(ShortTermMemories);

// public class Replay(ShortTermMemories shortTermMemories)
// {
//     private int currentIndex = 0;
//     public IEnumerable<int> Next()
//     {
//         if (currentIndex >= shortTermMemories.Count()) yield break;
//         yield return shortTermMemories.Numbers[currentIndex];
//         currentIndex++;
//     }
//     public void StoreAction(string key) => shortTermMemories.GetMemory(shortTermMemories.Numbers[currentIndex]).StoreAction(key);

//     public static Replay Empty => new(new ShortTermMemories());
// }