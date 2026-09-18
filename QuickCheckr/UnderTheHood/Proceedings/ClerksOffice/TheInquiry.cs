using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

internal static class TheInquiry
{
    public static Flow<Flow> Flow(Inquiry input) =>
        Style
            .DrawTopLine()
            .InquiryHeaderCount(input.NumberOfRuns, "Total", "Run")
            .InquiryHeaderCount(input.FailureCount, "Failed", "Run")
            .InquiryHeaderCount(input.DistinctFailureCount, "Distinct", "Failure")
            .DrawLine()
            .NewLine()
            .ToFlow(InquiryCaseFile, input.DistinctFailures)
            .NewLine()
            .DoubleLine()
            .OnNewLine().Trace("Passed Expectations")
            .ToFlow(InquiryPassedExpectations, input.PassedExpectations)
            .DoubleLine();

    private static Flow<Flow> InquiryHeaderCount(this Flow<Flow> other, int count, string label, string suffix) =>
        other.Then(
            Style
                .OnNewLine()
                .Trace(count)
                .Space()
                .Trace(label)
                .Space()
                .Pluralize(count, suffix));

    private static Flow<Flow> InquiryCaseFile(CaseFile caseFile) =>
        Style
            .DoubleLine()
            .OnNewLine()
                .CaptionWidth("Seed")
                .Trace(caseFile.Seed)
            .OnNewLine()
                .CaptionWidth("Original failing run")
                .Trace(caseFile.OriginalRunExecutionCount)
                .Space()
                .Pluralize(caseFile.OriginalRunExecutionCount, "execution")
            .OnNewLine().CaptionWidth("Failure").Trace(caseFile.FailureDeposition.GetFailureDescription())
            .When(caseFile.ShrinkCount > 0,
                Style
                    .OnNewLine()
                    .CaptionWidth("Minimal failing case")
                    .Trace(caseFile.ExecutionCount)
                    .Space()
                    .Pluralize(caseFile.ExecutionCount, "execution")
                    .Space()
                    .Parenthesized(
                        Pulse
                            .Trace("after").Space()
                            .Trace(caseFile.ShrinkCount).Space()
                            .Pluralize(caseFile.ShrinkCount, "shrink")))
            .ToFlow(TheCourtStyleGuide.Execution, caseFile.CollapseExecutions())
            .DoubleLine()
            .NewLine();

    private static Flow<Flow> InquiryPassedExpectations(IEnumerable<PassedExpectation> input) =>
        Pulse
            .ToFlow(a => Style.OnNewLine().Trace($"- {a.Label}: {a.TimesPassed}x"), input);
}
