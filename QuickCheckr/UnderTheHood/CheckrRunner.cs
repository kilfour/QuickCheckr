using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Runners;
using QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;
using QuickCheckr.UnderTheHood.Runners.InputShrinking;
using QuickCheckr.UnderTheHood.Runners.Tracing;

namespace QuickCheckr.UnderTheHood;

public class CheckrRunner(CheckrOf<Case> check, CheckrRunnerConfig config, int numberOfExecutions)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(numberOfExecutions, config.MaxStepsPerReducer, config.Seed);

    public int Seed => State.FuzzrState.Seed;

    public CaseFile Run()
    {
        new InitialRun(State, Check).Run(numberOfExecutions);
        if (State.EvidenceRecorder.Failed())
            return HandleFailure();
        return Compile.PassedExpectations(State.EvidenceRecorder.PassedExpectations);
    }

    private CaseFile HandleFailure()
    {
        var originalRunExecutionCount = State.Memory.Count();
        var shrinkCount = 0;

        if (!string.IsNullOrWhiteSpace(State.EvidenceRecorder.FailingVerify))
            return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);

        if (config.ShrinkMode.HasFlag(ShrinkMode.Executions))
            shrinkCount += new ExecutionShrinker(State, Check).Run();

        // if (config.ShrinkMode.HasFlag(ShrinkMode.Actions))
        //     shrinkCount += new ActionShrinker(State, Check).Run();

        new TraceRun(State, Check).Run(config.ReportMode.HasFlag(ReportMode.Trace));

        if (config.ShrinkMode.HasFlag(ShrinkMode.Inputs))
            shrinkCount += new InputShrinker(State, Check, config.ShrinkMode.HasFlag(ShrinkMode.Reduction)).Run();

        if (config.ReportMode.HasFlag(ReportMode.FinalTrace))
            new FinalTraceRun(State, Check).Run();

        return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
    }

    private CaseFile CompileTheCaseFile(
        int originalRunExecutionCount,
        int shrinkCount)
    {
        var dossier =
            new Dossier(
                FailureInfo:
                    new FailureInfo(
                        State.EvidenceRecorder.FailingExpectation,
                        State.EvidenceRecorder.FailingVerify,
                        State.EvidenceRecorder.Exception,
                        config.ReportMode.HasFlag(ReportMode.StackTrace)),
                RunInfo:
                    new RunInfo(
                        originalRunExecutionCount,
                        shrinkCount,
                        State.FuzzrState.Seed),
                PassedExpectations: State.EvidenceRecorder.PassedExpectations,
                UncheckedExpectations: GetUncheckedExpectations(),
                UseMemoryForInputReporting: !config.ShrinkMode.HasFlag(ShrinkMode.Inputs),
                ReportMode: config.ReportMode
            );
        return Compile.TheCaseFile(dossier, State.Memory, State.ClassificationFormatter.InputPrinter);
    }

    private List<string> GetUncheckedExpectations()
    {
        return [.. State.Memory.KeyGuardian.Expects.KeysWritten
            .Where(a =>
                a != State.EvidenceRecorder.FailingExpectation &&
                a != State.EvidenceRecorder.FailingVerify &&
                !State.EvidenceRecorder.PassedExpectations.ContainsKey(a))];
    }
}