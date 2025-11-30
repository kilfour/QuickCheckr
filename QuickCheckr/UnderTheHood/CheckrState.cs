using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories;
using QuickFuzzr.UnderTheHood;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrState(int desiredNumberOfExecutions, Signal<IFiling>[] signals, int? seed = null)
{
    public State FuzzrState { get; init; } = seed is null ? new() : new(seed.Value);
    public Memory Memory { get; init; } = new();
    public EvidenceRecorder EvidenceRecorder { get; init; } = new();
    public Docket Docket { get; init; } = new(signals);

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

    public bool IsThisTheRunsLastExecution()
    {
        if (EvidenceRecorder.IsReplay)
            return Memory.ShortTermMemory.CurrentExecutionNumber
                == Memory.ShortTermMemory.Numbers[Memory.ShortTermMemory.Numbers.Count - 1];
        return Memory.ShortTermMemory.CurrentExecutionNumber == desiredNumberOfExecutions;
    }

    private readonly Dictionary<Type, object> substitutes = new();
    public void RegisterSubstitute<T>(T value)
        => substitutes[typeof(T)] = value!;

    public object? GetSubstitute(Type type, object original)
        => substitutes.TryGetValue(type, out object? value) ? value : original;
}