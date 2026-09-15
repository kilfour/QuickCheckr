using QuickCheckr.Diagnostics;
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

internal sealed class CheckrRunner(CheckrOf<Case> check, CheckrRunnerConfig config, int numberOfExecutions, AutopsyProbe autopsyProbe)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(numberOfExecutions, config.MaxStepsPerReducer, config.Seed, autopsyProbe);

    public int Seed => State.FuzzrState.Seed;

    // Step 1 adapter: replay and shrinking become awaitable in the next migration step.
    public ValueTask<CaseFile> RunAsync() => new(Run());

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

        if (!string.IsNullOrWhiteSpace(State.EvidenceRecorder.FailingVerification.Key))
        {
            RunCaseSelection();
            return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
        }

        if (config.ShrinkMode.HasFlag(ShrinkMode.Executions))
            shrinkCount += new ExecutionShrinker(State, Check).Run();

        if (config.ShrinkMode.HasFlag(ShrinkMode.Actions))
            shrinkCount += new ActionShrinker(State, Check).Run();

        new TraceRun(State, Check).Run(config.ReportMode.HasFlag(ReportMode.Trace));

        if (config.ShrinkMode.HasFlag(ShrinkMode.Inputs))
            shrinkCount += new InputShrinker(State, Check, config.ShrinkMode.HasFlag(ShrinkMode.Reduction)).Run();

        if (config.ReportMode.HasFlag(ReportMode.FinalTrace))
            new FinalTraceRun(State, Check).Run();

        RunCaseSelection();

        return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
    }

    private void RunCaseSelection()
    {
        var deliberate = State.Memory.HasDeliberation;
        var distinguish = State.Memory.HasDistinction;
        var rejectCases = State.Memory.HasCaseRejection;
        if (deliberate || distinguish || rejectCases)
            new CaseSelectionRun(State, Check, deliberate, distinguish, rejectCases).Run();
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
                        State.EvidenceRecorder.FailingVerification,
                        State.EvidenceRecorder.Exception),
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
        var caseFile = Compile.TheCaseFile(
            dossier,
            State.Memory,
            State.ClassificationFormatter.InputPrinter);
        var selection = State.Memory.GetCaseSelection();
        if (selection is not null &&
            (selection.ExecutionDeliberationScore.HasValue || selection.RunDeliberationScores.Count != 0))
        {
            var score = selection.ExecutionDeliberationScore ?? 0;
            score += selection.RunDeliberationScores.Sum(calculate => calculate(caseFile));
            caseFile.AddDeliberation(score, selection.DeliberationTarget);
        }
        if (selection?.Distinction is { } distinction)
            caseFile.AddDistinction(distinction);
        if (selection?.RejectCase == true)
            caseFile.Reject();
        return caseFile;
    }
}
