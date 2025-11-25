using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Protocol;
using QuickCheckr.UnderTheHood.Runners;
using QuickCheckr.UnderTheHood.Runners.ActionShrinking;
using QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking;

namespace QuickCheckr.UnderTheHood;

public class CheckrRunner(CheckrOf<Case> check, CheckrRunnerConfig config)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(config.Signal, config.Seed);

    public CaseFile Run(int numberOfExecutions)
    {
        new InitialRun(State, Check).Run(numberOfExecutions);
        if (State.EvidenceRecorder.Failed()) return HandleFailure();
        return CaseFile.Empty();
    }

    private CaseFile HandleFailure()
    {
        var originalRunExecutionCount = State.ShortTermMemories.Count();
        var shrinkCount = 0;
        if (!config.NoShrinking)
        {
            shrinkCount += new ExecutionShrinker(State, Check).Run();
            if (config.ShrinkingActions)
                shrinkCount += new ActionShrinker(State, Check).Run();
            shrinkCount += new InputShrinker(State, Check).Run();
            shrinkCount += new InputReducer(State, Check).Run();
        }
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
                ExecutionNumbers: State.ShortTermMemories.Numbers,
                ShrinkCount: shrinkCount,
                Seed: State.FuzzrState.Seed
            );
        return Compile.TheCaseFile(dossier, State.ShortTermMemories);
    }
}