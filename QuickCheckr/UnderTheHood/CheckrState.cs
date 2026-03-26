using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood;

public class CheckrState(
    int desiredNumberOfExecutions,
    int? seed = null)
{
    public State FuzzrState { get; init; } = seed is null ? new() : new(seed.Value);
    public Memory Memory { get; init; } = new();
    public EvidenceRecorder EvidenceRecorder { get; init; } = new();
    public ManOfWealthAndTaste Editor { get; set; } =
        Please.AllowMe().ToUseNonLinearTime();
    public ManOfWealthAndTaste ReductionPrinter { get; set; } =
        Please.AllowMe().ToUseNonLinearTime();
    public ClassificationFormatter ClassificationFormatter { get; set; } = new();

    public int StartReplay()
    {
        EvidenceRecorder.StartReplay();
        return Memory.Reset();
    }

    private bool runStopped = false;
    public bool RunStopped => runStopped;
    public void StopRun() { runStopped = true; }

    public bool IsThisTheRunsLastExecution()
    {
        if (EvidenceRecorder.IsReplay)
            return Memory.ShortTermMemory.CurrentExecutionNumber
                == Memory.ShortTermMemory.Numbers[Memory.ShortTermMemory.Numbers.Count - 1];
        return Memory.ShortTermMemory.CurrentExecutionNumber == desiredNumberOfExecutions;
    }
}