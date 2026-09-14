using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.FilingCabinet.Depositions.Failure;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;

namespace QuickTestr.Bolts.ClerksOffice;

/// <summary>
/// Provides the built-in report style guides used by QuickTestr model-based tests.
/// Use when comparing a System Under Test (SUT) against a reference model.
/// </summary>
public class ModelClerk : ITranscribe
{
    /// <summary>
    /// Formats a case file using the model-based QuickTestr report style.
    /// Reports the minimal sequence of operations that caused the model
    /// and SUT to diverge.
    /// </summary>
    public Flow<Flow> File(IRecord record) =>
        record is CaseFile caseFile ?
            CaseFile(caseFile) :
            TheCourtStyleGuide.Record(record, new Decorum());

    private static Flow<Flow> CaseFile(CaseFile caseFile) =>
        Style
            .DrawTopLine()
            .OnNewLine()
                .Indent(1)
                .CaptionWidth("Falsified after")
                .Trace(caseFile.OriginalRunExecutionCount)
                .Space()
                .Pluralize(caseFile.OriginalRunExecutionCount, "execution")
            .OnNewLine()
                .Indent(1)
                .CaptionWidth("Minimal scenario")
                .Trace(caseFile.ExecutionCount)
                .Space()
                .Pluralize(caseFile.ExecutionCount, "execution")
            .OnNewLine()
                .Indent(1)
                .CaptionWidth("Seed")
                .Trace(caseFile.Seed)
            .DrawLine()
            .ToFlow(Execution, caseFile.ExecutionDepositions.Select((a, index) => (a, index)))
            .ToFlow(FailureFlow, caseFile.FailureDeposition)
            .ToFlow(Trace, caseFile.ExecutionDepositions.Last().TraceDepositions)
            .DrawLine();

    private static Flow<Flow> Execution((ExecutionDeposition Execution, int Index) input) =>
        Style
            .OnNewLine()
                .Indent(1)
                .Trace(input.Index + 1)
                .Trace(".")
                .Space()
                .Trace(input.Execution.ActionDepositions.First().Label[5..])
                .ToFlow(Input, input.Execution.InputDepositions);

    private static Flow<Flow> Input(InputDeposition input) =>
        Style
            .OnNewLine()
                .Indent(4)
                .Trace(input.Value);
    public static Flow<Flow> FailureFlow(FailureDeposition failure) =>
        Style
            .DrawLine()
            .OnNewLine()
            .Indent(1)
            .Trace("!! ")
            .OnType((FailedExpectationDeposition a) => FailedExpectationFlow(a), () => failure)
            .OnType((FailedExceptionDeposition a) => ExceptionFlow(a), () => failure)
            .NewLine();

    private static Flow<Flow> FailedExpectationFlow(FailedExpectationDeposition failure)
        => Style.Caption("Failed").Space().Trace(failure.FailedExpectation);

    private static Flow<Flow> ExceptionFlow(FailedExceptionDeposition failure)
        => Pulse.Trace(failure.GetFailureDescription());

    private static Flow<Flow> Trace(TraceDeposition input) =>
        Style
            .OnNewLine()
                .Indent(4)
                .Trace(input.Label)
                .Space()
                .Trace(input.Value);
}
