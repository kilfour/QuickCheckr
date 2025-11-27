using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Docketing;


public class Docket(Signal<IFiling>? signal = null)
{
    private readonly Signal<IFiling> signal =
        signal ?? Signal.From<IFiling>(_ => Pulse.NoOp());

    public void Record(IFiling filing) => signal.Pulse(filing);
}

public record struct ExecutionRemoved(int ExecutionId) : IFiling;
public record struct ActionRemoved(int ExecutionId, string Key) : IFiling;
public record struct InputShrinkingClueFound(InputShrinkingClue Clue) : IFiling;
public record struct InputReduced(int ExecutionId, string Key) : IFiling;
public record struct EvidenceException(Exception Exception) : IFiling;