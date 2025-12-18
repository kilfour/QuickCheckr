using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Docketing;


public class Docket(Signal<IFiling>[] signals)
{
    public void Record(IFiling filing) { foreach (var signal in signals) signal.Pulse(filing); }
}

public record struct ExecutionRemoved(int ExecutionId) : IFiling;
public record struct ActionRemoved(int ExecutionId, string Key) : IFiling;
public record struct InputShrinkingClueFound(InputShrinkingClue Clue) : IFiling;
public record struct InputReduced(int ExecutionId, string Key) : IFiling;
public record struct EvidenceFailure(Exception Exception) : IFiling;
public record struct SpecCaughtActionWasNotExecuted(string ExpectKey) : IFiling;