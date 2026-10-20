using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Represents the execution context passed between custom Checkr steps.
/// </summary>
public sealed class CheckrState
{
    public CancellationToken CancellationToken { get; }
    internal State FuzzrState { get; init; }
    internal Memory Memory { get; init; } = new();
    internal PoolIterationScope? PoolIteration { get; set; }
    internal EvidenceRecorder EvidenceRecorder { get; init; }
    internal ManOfWealthAndTaste Editor { get; set; } =
        Please.AllowMe().ToUseNonLinearTime();
    internal ManOfWealthAndTaste ReductionPrinter { get; set; } =
        Please.AllowMe().ToUseNonLinearTime();
    internal ClassificationFormatter ClassificationFormatter { get; set; } = new();

    internal int StartReplay()
    {
        CancellationToken.ThrowIfCancellationRequested();
        ExecutionAborted = false;
        EvidenceRecorder.StartReplay();
        return Memory.Reset();
    }

    internal bool IsInitialRun => !EvidenceRecorder.IsReplay;

    private bool runStopped = false;
    private readonly int desiredNumberOfExecutions;

    internal CheckrState(
        int desiredNumberOfExecutions,
        int maxStepsPerReducer,
        int? seed,
        AutopsyProbe autopsyProbe,
        CancellationToken cancellationToken = default)
    {
        this.desiredNumberOfExecutions = desiredNumberOfExecutions;
        CancellationToken = cancellationToken;
        FuzzrState = seed is null ? new() : new(seed.Value);
        MaxStepsPerReducer = maxStepsPerReducer;
        Pathologist = new Pathologist(autopsyProbe, this);
        EvidenceRecorder = new(Pathologist);
    }

    internal bool RunStopped => runStopped || ExecutionAborted;

    internal bool ExecutionAborted { get; private set; }

    internal void AbortExecution() => ExecutionAborted = true;

    internal int MaxStepsPerReducer { get; }

    internal void StopRun() { runStopped = true; }

    internal bool IsThisTheRunsLastExecution()
    {
        if (EvidenceRecorder.IsReplay)
            return Memory.ShortTermMemory.CurrentExecutionNumber
                == Memory.ShortTermMemory.Numbers[Memory.ShortTermMemory.Numbers.Count - 1];
        return Memory.ShortTermMemory.CurrentExecutionNumber == desiredNumberOfExecutions;
    }

    internal Pathologist Pathologist { get; }
}
