using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Runners;
using QuickCheckr.UnderTheHood.Runners.ActionShrinking;
using QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;
using QuickCheckr.UnderTheHood.Runners.InputShrinking;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrRunner(CheckrOf<Case> check, Signal<IFiling>? signal, int? seed = null)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(signal, seed);

    public CaseFile Run(int numberOfExecutions)
    {
        new InitialRun(State, Check).Run(numberOfExecutions);
        if (State.EvidenceRecorder.Failed()) return HandleFailure();
        return CaseFile.Empty();
    }

    private CaseFile HandleFailure()
    {
        var originalRunExecutionCount = State.ShortTermMemory.Count();
        var shrinkCount = new ExecutionShrinker(State, Check).Run();
        shrinkCount += new ActionShrinker(State, Check).Run();
        shrinkCount += new InputShrinker(State, Check).Run();
        return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
    }

    private CaseFile CompileTheCaseFile(
        int originalRunExecutionCount,
        int shrinkCount,
        bool forAssayer = false)
    {
        var dossier =
            new Dossier(
                AssayerSpec: forAssayer ? State.EvidenceRecorder.FailingSpec : null,
                FailingSpec: forAssayer ? null : State.EvidenceRecorder.FailingSpec,
                Exception: State.EvidenceRecorder.Exception,
                OriginalRunExecutionCount: originalRunExecutionCount,
                ExecutionNumbers: State.ShortTermMemory.Numbers,
                ShrinkCount: shrinkCount,
                Seed: State.FuzzrState.Seed
            );
        return Compile.TheCaseFile(dossier, State.ShortTermMemory);
    }
}