using QuickCheckr;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;
using QuickPulse;

namespace QuickCheckr.Invariants;

public static class TheTestr
{
    private readonly static Flow<Flow> space = Pulse.Trace(" ");

    private static readonly Flow<Flow> newLine = Pulse.Trace(Environment.NewLine);

    private static Flow<Flow> LineOf(int length) => Pulse.Trace(new string('-', length));

    private readonly static Flow<Flow> line =
        space.Then(LineOf(60)).Then(newLine);

    private readonly static Flow<WarningDeposition> warningDeposition =
        from input in Pulse.Start<WarningDeposition>()
        from _ in newLine
        from __ in Pulse.Trace($"   - WARNING: {input.Value}")
        select input;

    private readonly static Flow<InputDeposition> inputDeposition =
        from input in Pulse.Start<InputDeposition>()
        let label = input.Label
        from _2 in Pulse.Trace($"    Input = {input.Value}").Then(newLine)
        from _3 in Pulse.TraceIf(input.Redux.HasValue, () => $"    Redux = {input.Redux.Value}").Then(newLine)
        from _5 in Pulse.When(input.Original.HasValue,
            newLine
                .Then(Pulse.Trace("  Original:"))
                .Then(newLine)
                .Then(Pulse.Trace($"    {input.Original.Value!}")))
        select input;

    private readonly static Flow<ExecutionDeposition> executionDeposition =
        from execution in Pulse.Start<ExecutionDeposition>()
        from _1 in newLine
        from _2 in Pulse.ToFlow(inputDeposition, execution.InputDepositions)
        from _3 in Pulse.ToFlow(warningDeposition, execution.GetWarningDepositionsForReport())
        from _4 in newLine.Then(space).Then(LineOf(60))
        select execution;

    private readonly static Flow<FailedExpectationDeposition> failedExpectationDeposition =
        from input in Pulse.Start<FailedExpectationDeposition>()
        from _ in Pulse.Trace($"  {input.FailedExpectation}").Then(newLine)
        select input;

    private readonly static Flow<FailedExceptionDeposition> exceptionDeposition =
        from input in Pulse.Start<FailedExceptionDeposition>()
        from _ in Pulse.Trace($"  {input.Message}").Then(newLine)
        select input;

    private readonly static Flow<FailureDeposition> failureDeposition =
        Pulse.Start<FailureDeposition>(input =>
            Pulse.FirstOf(
                (() => input is FailedExpectationDeposition, () => Pulse.ToFlow(failedExpectationDeposition, (FailedExpectationDeposition)input)),
                (() => input is FailedExceptionDeposition, () => Pulse.ToFlow(exceptionDeposition, (FailedExceptionDeposition)input))));

    private static string Pluralize(int count, string str) =>
        count > 1 ? $"{str}s" : str;

    private readonly static Flow<CaseFile> evidence =
        from caseFile in Pulse.Start<CaseFile>()
        from _1 in line
        from _2 in Pulse.ToFlow(failureDeposition, caseFile.FailureDeposition)
        from _3 in Pulse.Trace($"  Seed: {caseFile.Seed}").Then(newLine)
        from _4 in line
        from _5 in Pulse.Trace("  Falsified:")
        from _6 in Pulse.ToFlow(executionDeposition, caseFile.ExecutionDepositions)
        select caseFile;

    private readonly static Flow<CaseSummary> Summary =
        from input in Pulse.Start<CaseSummary>()
        let runs = $" {input.NumberOfRuns} {Pluralize(input.NumberOfRuns, "Run")}{Environment.NewLine}"
        from _ in Pulse.Trace(runs)
        select input;

    public readonly static Flow<CaseFile> StyleGuide =
        from input in Pulse.Start<CaseFile>()
        from _ in Pulse.ToFlowIf(input.HasEvidence, evidence, () => input)
        from summary in Pulse.OnType(Summary, () => input)
        select input;
}