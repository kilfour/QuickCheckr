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
    public CheckrState State { get; init; } = new(numberOfExecutions, config.MaxStepsPerReducer, config.Seed, autopsyProbe, config.CancellationToken);

    public int Seed => State.FuzzrState.Seed;

    public CaseFile Run()
        => RunAsync().GetAwaiter().GetResult();

    public async ValueTask<CaseFile> RunAsync()
    {
        await new InitialRun(State, Check).RunAsync(numberOfExecutions).ConfigureAwait(false);
        if (State.EvidenceRecorder.Failed())
            return await HandleFailureAsync().ConfigureAwait(false);
        var passedExpectations = State.EvidenceRecorder.PassedExpectations
            .Select(a => new PassedExpectation(a.Key, a.Value));
        return CaseFile.Passed(State.FuzzrState.Seed, [.. passedExpectations]);
    }

    private async ValueTask<CaseFile> HandleFailureAsync()
    {
        var originalRunExecutionCount = State.Memory.Count();
        var shrinkCount = 0;

        if (!string.IsNullOrWhiteSpace(State.EvidenceRecorder.FailingVerification.Key))
        {
            await RunCaseSelectionAsync().ConfigureAwait(false);
            return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
        }

        if (config.ShrinkMode.HasFlag(ShrinkMode.Executions))
            shrinkCount += await new ExecutionShrinker(State, Check).RunAsync().ConfigureAwait(false);

        if (config.ShrinkMode.HasFlag(ShrinkMode.Actions))
            shrinkCount += await new ActionShrinker(State, Check).RunAsync().ConfigureAwait(false);

        await new TraceRun(State, Check).RunAsync(config.ReportMode.HasFlag(ReportMode.Trace)).ConfigureAwait(false);

        if (config.ShrinkMode.HasFlag(ShrinkMode.Inputs))
            shrinkCount += await new InputShrinker(State, Check, config.ShrinkMode.HasFlag(ShrinkMode.Reduction)).RunAsync().ConfigureAwait(false);

        if (config.ReportMode.HasFlag(ReportMode.FinalTrace))
            await new FinalTraceRun(State, Check).RunAsync().ConfigureAwait(false);

        await RunCaseSelectionAsync().ConfigureAwait(false);

        return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
    }

    private async ValueTask RunCaseSelectionAsync()
    {
        var deliberate = State.Memory.HasDeliberation;
        var distinguish = State.Memory.HasDistinction;
        var rejectCases = State.Memory.HasCaseRejection;
        if (deliberate || distinguish || rejectCases)
            await new CaseSelectionRun(State, Check, deliberate, distinguish, rejectCases).RunAsync().ConfigureAwait(false);
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
