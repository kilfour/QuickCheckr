using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood;

public class CheckrState
{
    public State FuzzrState { get; init; }
    public Memory Memory { get; init; } = new();
    public EvidenceRecorder EvidenceRecorder { get; init; }
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
    private readonly int desiredNumberOfExecutions;

    public CheckrState(
        int desiredNumberOfExecutions,
        int maxStepsPerReducer,
        int? seed,
        Microtome microtome)
    {
        this.desiredNumberOfExecutions = desiredNumberOfExecutions;
        FuzzrState = seed is null ? new() : new(seed.Value);
        MaxStepsPerReducer = maxStepsPerReducer;
        Pathologist = new Pathologist(microtome);
        EvidenceRecorder = new(Pathologist);
    }

    public bool RunStopped => runStopped;

    public int MaxStepsPerReducer { get; }

    public void StopRun() { runStopped = true; }

    public bool IsThisTheRunsLastExecution()
    {
        if (EvidenceRecorder.IsReplay)
            return Memory.ShortTermMemory.CurrentExecutionNumber
                == Memory.ShortTermMemory.Numbers[Memory.ShortTermMemory.Numbers.Count - 1];
        return Memory.ShortTermMemory.CurrentExecutionNumber == desiredNumberOfExecutions;
    }

    public Pathologist Pathologist { get; }
}