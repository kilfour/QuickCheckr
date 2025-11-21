using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Docketing;


public class Docket(Signal<IFiling>? signal = null)
{
    private readonly Signal<IFiling> signal =
        signal ?? Signal.From<IFiling>(_ => Pulse.NoOp());

    public void Record(IFiling filing) => signal.Pulse(filing);
}

public record struct InitialRunStarted(int DesiredNumberOfExecutions) : IFiling;
public record struct InitialRunEnded(int PerformedNumberOfExecutions) : IFiling;
public record struct ExecutionStarted(int CurrentExecution) : IFiling;
public record struct ExecutionFinished(int CurrentExecution) : IFiling;


public record struct ExecutionShrinkingStarted(int NumberOfExecutions) : IFiling;
public record struct ExecutionShrinkRunStarted(int NumberOfExecutions) : IFiling;
public record struct ExecutionRemoved(int ExecutionId) : IFiling;
public record struct ExecutionShrinkRunEnded(int NumberOfExecutions) : IFiling;
public record struct ExecutionShrinkingEnded(int NumberOfExecutions) : IFiling;

public record struct ActionShrinkingStarted(int NumberOfExecutions) : IFiling;
public record struct ActionShrinkRunStarted(int NumberOfExecutions) : IFiling;
public record struct ActionRemoved(int ExecutionId, string Key) : IFiling;
public record struct ActionShrinkRunEnded(int NumberOfExecutions) : IFiling;
public record struct ActionShrinkingEnded(int NumberOfExecutions) : IFiling;


public record struct InputShrinkingStarted(int NumberOfExecutions) : IFiling;
public record struct InputShrinkRunStarted(int ExecutionId, string Key) : IFiling;
public record struct InputShrinkingClueFound(InputShrinkingClue Clue) : IFiling;
public record struct InputShrunk(int ExecutionId, string Key) : IFiling;
public record struct InputShrinkRunEnded(int ExecutionId, string Key) : IFiling;
public record struct InputShrinkingEnded(int NumberOfExecutions) : IFiling;