using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Runners;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public class CheckrRunner(CheckrOf<Case> check, Signal<IFiling>? signal)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(signal);

    public CaseFile Run(int numberOfExecutions)
    {
        var executions = new InitialRun(State, Check).Run(numberOfExecutions);
        if (State.EvidenceRecorder.Failed()) return HandleFailure(executions);
        return CaseFile.Empty();
    }

    private CaseFile HandleFailure(Executions executions)
    {
        var originalRunExecutionCount = executions.Count();
        var shrinkCount = new ExecutionShrinker(State, Check, executions).Run();
        return CompileTheCaseFile(executions, originalRunExecutionCount, shrinkCount);
    }

    private CaseFile CompileTheCaseFile(
        Executions executions,
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
                ExecutionNumbers: executions.Numbers,
                ShrinkCount: shrinkCount,
                Seed: State.FuzzrState.Seed
            );
        return Compile.TheCaseFile(dossier, State.ShortTermMemory);
    }
}