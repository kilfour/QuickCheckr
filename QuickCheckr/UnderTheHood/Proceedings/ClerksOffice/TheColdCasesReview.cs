using QuickCheckr.FilingCabinet;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

internal static class TheColdCasesReview
{
    public static Flow<Flow> Flow(ColdCasesReview input) =>
        Style
            .DrawTopLine()
            .OnNewLine().Trace(input.NumberOfRuns).Space().Pluralize(input.NumberOfRuns, "Run")
            .DrawLine()
            .ToFlow(CaseFile, input.CaseFiles)
            .OnNewLine().Trace("Passed Expectations")
            .ToFlow(TheCourtStyleGuide.PassedExpectations, input.PassedExpectationDepositions);

    private static Flow<Flow> CaseFile(CaseFile caseFile) =>
        Style
            .OnNewLine()
                .CaptionWidth("Seed")
                .Trace(caseFile.Seed)
            .OnNewLine()
                .CaptionWidth("Result")
                .Trace(caseFile.HasEvidence ? "Failed" : "Passed")
            .DrawLine();
}
