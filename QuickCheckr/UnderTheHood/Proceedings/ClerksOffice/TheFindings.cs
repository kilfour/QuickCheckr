using QuickCheckr.FilingCabinet;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class TheFindings
{
    public static Flow<Flow> Flow(Findings input) =>
        Style
            .DrawTopLine()
            .OnNewLine().Trace(input.NumberOfRuns).Space().Pluralize(input.NumberOfRuns, "Run")
            .DrawLine()
            .OnNewLine().Trace("Passed Expectations")
            .ToFlow(TheCourtStyleGuide.PassedExpectations, input.PassedExpectationDepositions);
}