using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;

namespace QuickTestr.Bolts.ClerksOffice;


/// <summary>
/// Provides the built-in report style guides used by QuickTestr.
/// Use when you want the standard compact formatting for oracle-based cases.
/// </summary>
public class OracleClerk : ITranscribe
{
    /// <summary>
    /// Formats a case file using the oracle QuickTestr report style.
    /// Use for expected-versus-actual Testr output.
    /// </summary>
    public Flow<Flow> File(IRecord record) =>
        CommonClerk.Render(record, ExecutionFlow, Inquiry);

    private static Flow<Flow> Inquiry(Inquiry inquiry) =>
        Style
            .DrawTopLine()
            .InquiryHeaderCount(inquiry.NumberOfRuns, "Total", "Run")
            .InquiryHeaderCount(inquiry.FailureCount, "Failed", "Run")
            .InquiryHeaderCount(inquiry.DistinctFailureCount, "Distinct", "Failure")
            .When(inquiry.MaxStoredCaseFilesReached,
                Style
                    .OnNewLine()
                    .Trace("Max Stored Failures Reached"))
            .DrawLine()
            .NewLine()
            .ToFlow(InquiryCaseFile, inquiry.DistinctFailures)
            .NewLine()
            .DoubleLine()
            .OnNewLine().Trace("Passed Expectations")
            .ToFlow(InquiryPassedExpectations, inquiry.PassedExpectations)
            .DoubleLine();

    public static Flow<Flow> InquiryCaseFile(CaseFile caseFile) =>
        Style
            .DoubleLine()
            .OnNewLine().Trace($"Seed:                    {caseFile.Seed}")
            .OnNewLine().Trace($"Failure:                 {caseFile.FailureDeposition.GetFailureDescription()}")
            .ToFlow(ExecutionFlow, caseFile.ExecutionDepositions)
            .DoubleLine()
            .NewLine();

    private static Flow<Flow> InquiryPassedExpectations(IEnumerable<PassedExpectation> input) =>
        Pulse
            .ToFlow(a => Style.OnNewLine().Trace($"- {a.Label}: {a.TimesPassed}x"), input);

    private static Flow<Flow> ExecutionFlow(ExecutionDeposition execution) =>
        Pulse
            .ToFlowIf(execution.InputDepositions.Count != 0, OracleInputFlow, () => GetInputAndTraces(execution))
            .ToFlow(CommonClerk.WarningsFlow, execution.GetWarningDepositionsForReport());

    private record OracleInput(InputDeposition Input, List<TraceDeposition> Traces, List<TraceDeposition> FinalTraces);

    private static OracleInput GetInputAndTraces(ExecutionDeposition execution) =>
        new(execution.InputDepositions.Single(), execution.TraceDepositions, execution.FinalTraceDepositions);

    private static Flow<Flow> OracleInputFlow(OracleInput oracleInput) =>
        Style
            .NewLine()
            .OnNewLine()
            .Indent(3)
            .LabeledValue("Input", oracleInput.Input.Value)
            .ToFlow(TracesFlow, oracleInput.Traces)
            .When(oracleInput.Input.Redux.HasValue,
                Style
                    .NewLine()
                    .OnNewLine()
                    .Indent(3)
                    .LabeledValue("Redux", oracleInput.Input.Redux.Value!)
                    .ToFlow(TracesFlow, oracleInput.FinalTraces))
            .When(oracleInput.Input.Original.HasValue && !Equals(oracleInput.Input.Value, oracleInput.Input.Original.Value),
                Style
                    .NewLine()
                    .OnNewLine()
                    .Indent(1)
                    .Caption("Original")
                    .OnNewLine()
                    .Indent(3)
                    .Trace(oracleInput.Input.Original.Value!));

    private static Flow<Flow> TracesFlow(List<TraceDeposition> traces) =>
        Pulse.When(traces.Count > 0,
            Style
                .OnNewLine()
                .Indent(5)
                .Caption("Observed"))
                .ToFlow(TraceFlow, traces);

    private static Flow<Flow> TraceFlow(TraceDeposition trace)
        => Style
            .OnNewLine()
            .Indent(7)
            .LabeledValue(trace.Label, trace.Value);
}
