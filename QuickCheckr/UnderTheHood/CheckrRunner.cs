using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Runners;
using QuickCheckr.UnderTheHood.Runners.ActionShrinking;
using QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;
using QuickCheckr.UnderTheHood.Runners.InputShrinking;
using QuickCheckr.UnderTheHood.Runners.Tracing;

namespace QuickCheckr.UnderTheHood;

public class CheckrRunner(CheckrOf<Case> check, CheckrRunnerConfig config, int numberOfExecutions, List<string> focuses)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(numberOfExecutions, config.MaxStepsPerReducer, config.Seed, focuses);

    public int Seed => State.FuzzrState.Seed;

    public CaseFile Run()
    {
        new InitialRun(State, Check).Run(numberOfExecutions);
        if (State.EvidenceRecorder.Failed())
            return HandleFailure();
        var passedExpectations = State.EvidenceRecorder.PassedExpectations
            .Select(a => new PassedExpectation(a.Key, a.Value));
        return CaseFile.Passed(State.FuzzrState.Seed, [.. passedExpectations]);
    }

    private CaseFile HandleFailure()
    {
        var originalRunExecutionCount = State.Memory.Count();
        var shrinkCount = 0;

        if (!string.IsNullOrWhiteSpace(State.EvidenceRecorder.FailingVerify))
            return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);

        if (config.ShrinkMode.HasFlag(ShrinkMode.Executions))
            shrinkCount += new ExecutionShrinker(State, Check).Run();

        if (config.ShrinkMode.HasFlag(ShrinkMode.Actions))
            shrinkCount += new ActionShrinker(State, Check).Run();

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
                UseMemoryForInputReporting: !config.ShrinkMode.HasFlag(ShrinkMode.Inputs),
                ReportMode: config.ReportMode,
                WarningLevel: config.WarningLevel
            );
        return Compile.TheCaseFile(dossier, State.Memory, State.ClassificationFormatter.InputPrinter);
    }
}