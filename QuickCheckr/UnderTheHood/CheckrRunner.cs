using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Protocol;
using QuickCheckr.UnderTheHood.Runners;
using QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking;

namespace QuickCheckr.UnderTheHood;

public class CheckrRunner(CheckrOf<Case> check, CheckrRunnerConfig config, int numberOfExecutions)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(numberOfExecutions, config.ManOfWealthAndTaste, config.Signals, config.Seed);

    public int Seed => State.FuzzrState.Seed;

    public CaseFile Run()
    {
        new InitialRun(State, Check).Run(numberOfExecutions);
        if (State.EvidenceRecorder.Failed())
            return HandleFailure();
        return Compile.PassedSpecs(State.EvidenceRecorder.PassedSpecs);
    }

    private CaseFile HandleFailure()
    {
        var originalRunExecutionCount = State.Memory.Count();
        var shrinkCount = 0;

        if (!string.IsNullOrWhiteSpace(State.EvidenceRecorder.FailingAssay))
            return CompileTheCaseFile(originalRunExecutionCount, shrinkCount, true);

        if (config.ShrinkMode.HasFlag(ShrinkMode.Executions))
            shrinkCount += new ExecutionShrinker(State, Check).Run();

        // if (config.ShrinkMode.HasFlag(ShrinkMode.Actions))
        //     shrinkCount += new ActionShrinker(State, Check).Run();

        if (config.ShrinkMode.HasFlag(ShrinkMode.Inputs))
            shrinkCount += new InputShrinker(State, Check).Run();

        if (config.ShrinkMode.HasFlag(ShrinkMode.Reduction))
            shrinkCount += new InputReducer(State, Check).Run();

        return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
    }

    private CaseFile CompileTheCaseFile(
        int originalRunExecutionCount,
        int shrinkCount,
        bool forAssayer = false)
    {
        var dossier =
            new Dossier(
                AssayerSpec: forAssayer ? State.EvidenceRecorder.FailingAssay : null,
                FailingSpec: forAssayer ? null : State.EvidenceRecorder.FailingSpec,
                Exception: State.EvidenceRecorder.Exception,
                OriginalRunExecutionCount: originalRunExecutionCount,
                ExecutionCount: State.Memory.Count(),
                ShrinkCount: shrinkCount,
                Seed: State.FuzzrState.Seed,
                PassedSpecs: State.EvidenceRecorder.PassedSpecs
            );
        return Compile.TheCaseFile(dossier, State.Memory);
    }
}