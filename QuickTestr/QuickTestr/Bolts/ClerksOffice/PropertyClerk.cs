using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;

namespace QuickTestr.Bolts.ClerksOffice;


/// <summary>
/// Provides the built-in report style guides used by QuickTestr.
/// Use when you want the standard compact formatting for property-based cases.
/// </summary>
public class PropertyClerk : ITranscribe
{
    /// <summary>
    /// Formats a case file using the property-based QuickTestr report style.
    /// Use for standard property-based Testr output.
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
            .ToFlow(InputFlow, execution.InputDepositions)
            .ToFlow(CommonClerk.WarningsFlow, execution.GetWarningDepositionsForReport());

    private static Flow<Flow> InputFlow(InputDeposition input) =>
        Style
            .OnNewLine().Indent(3).LabeledValue("Input", input.Value)
            .When(input.Redux.HasValue,
                Style
                    .OnNewLine()
                    .Indent(3).LabeledValue("Redux", input.Redux.Value!))
            .When(input.Original.HasValue && !Equals(input.Value, input.Original.Value),
                Style
                    .NewLine()
                    .OnNewLine().Indent(1).Caption("Original")
                    .OnNewLine().Indent(3).Trace(input.Original.Value!));
}