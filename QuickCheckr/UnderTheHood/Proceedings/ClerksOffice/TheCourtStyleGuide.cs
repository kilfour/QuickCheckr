using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.FilingCabinet.Depositions.Failure;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class TheCourtStyleGuide
{
    private record MaxLabel(int Length);

    private static Flow<Flow> Warning(WarningDeposition input) =>
        Style.OnNewLine()
            .Indent(2)
            .Dash()
            .Space()
            .Caption("WARNING")
            .Space()
            .Trace(input.Value);

    private static Flow<Flow> Labeled(string type, string label, string value) =>
        from maxLabel in Pulse.Draw<MaxLabel>()
        from labeled in Pulse.Draw<Decorum, bool>(a => a.Labels)
        from _ in
            Style.OnNewLine()
                .Indent(2)
                .Trace("-")
                .Space()
                .When(labeled, Style.Caption(type).Space())
                .Trace($"{label.PadRight(maxLabel.Length, ' ')} = {value}")
        select QuickPulse.Flow.Continue;

    private static Flow<Flow> Trace(TraceDeposition input) =>
        Labeled("Trace", input.Label, input.Value);

    private static Flow<Flow> FinalTrace(TraceDeposition input) =>
        Labeled("Final", input.Label, input.Value);

    private static Flow<Flow> PoolTrace(PoolTraceDeposition input) =>
        Labeled(" Pool", input.Label, input.Value);

    private static Flow<Flow> Redux(InputDeposition input) =>
        from labeled in Pulse.Draw<Decorum, bool>(a => a.Labels)
        let leftPadding = labeled ? 11 : 4
        from maxLabel in Pulse.Draw<MaxLabel>()
        from _ in Style
            .OnNewLine()
            .Trace(new string(' ', leftPadding))
            .Trace("Redux")
            .Trace(new string(' ', Math.Max(3, maxLabel.Length - 2)))
            .Trace(input.Redux.Value!)
        select QuickPulse.Flow.Continue;

    private static Flow<Flow> Input(InputDeposition input) =>
        from labeled in Pulse.Draw<Decorum, bool>(a => a.Labels)
        let spaces = labeled ? 14 : 7
        from maxLabel in Pulse.Draw<MaxLabel>()
        from _ in Style
            .OnNewLine()
            .Indent(2)
            .Dash()
            .Space()
            .When(labeled, Style.Caption("Input").Space())
            .Trace($"{input.Label.PadRight(maxLabel.Length, ' ')} = {input.Value}")
            .When(input.Original.HasValue,
                Style.OnNewLine()
                    .Trace(new string(' ', maxLabel.Length + spaces))
                    .Trace(input.Original.Value!))
            .ToFlowIf(input.Redux.HasValue, Redux, () => input)
        select QuickPulse.Flow.Continue;

    private static Flow<Flow> Actions(IEnumerable<ActionDeposition> input) =>
        Style.Interspersed(input.Select(a => a.Label));

    private static Flow<Flow> Actions(this Flow<Flow> other, IEnumerable<ActionDeposition> input) =>
        other.Then(Actions(input));

    private static Flow<Flow> Execution(CollapsableExecutionDeposition input, ExecutionDeposition execution) =>
        Pulse
            .Manipulate<MaxLabel>(a => a with { Length = execution.MaxLabelLength() }).Dissipate()
            .DrawLine()
            .OnNewLine().Indent(1).Trace("Executed").TraceIf(execution.HasContent(), () => ":")
            .TraceIf(execution.ActionDepositions.Count > 0, () => " ")
            .Actions(execution.ActionDepositions)
            .TraceIf(input.Times > 1, () => $" ({input.Times} Times)")
            .ToFlow(PoolTrace,  /**/ execution.PoolTraceDepositions)
            .ToFlow(Input,      /**/ execution.InputDepositions)
            .ToFlow(Trace,      /**/ execution.TraceDepositions)
            .ToFlow(FinalTrace, /**/ execution.FinalTraceDepositions)
            .ToFlow(Warning,    /**/ execution.GetWarningDepositionsForReport());

    private static Flow<Flow> Execution(CollapsableExecutionDeposition input) =>
        Execution(input, input.ExecutionDeposition);

    private static Flow<Flow> FailedVerification(FailedVerificationDeposition input)
    {
        var text = $"The Verifier Disagrees: {input.FailedExpectation}";
        var length = text.Length + 7;
        return Style
            .DoubleLineOf(length)
            .OnNewLine().Indent(1).Exclaim(text)
            .DoubleLineOf(length);
    }

    private static Flow<Flow> FailedExpectationMessage(string message)
        => Style.OnNewLine().Indent(6).Trace(message);

    private static Flow<Flow> FailedExpectation(FailedExpectationDeposition input)
    {
        var text = $"Expectation Failed: {input.FailedExpectation}";
        var length = text.Length + 7;
        return Style
            .DoubleLineOf(length)
            .OnNewLine().Indent(1).Exclaim(text)
            .ToFlow(FailedExpectationMessage, input.Messages)
            .DoubleLineOf(length);
    }

    private static Flow<Flow> Exception(FailedExceptionDeposition input) =>
        Style
            .DoubleLineOf(75)
            .OnNewLine().Indent(1).Exclaim(input.Message)
            .DoubleLineOf(75);

    private static Flow<Flow> Failure(FailureDeposition input) =>
        Pulse
            .OnType((FailedExpectationDeposition a) => FailedExpectation(a), () => input)
            .OnType((FailedExceptionDeposition a) => Exception(a), () => input)
            .OnType((FailedVerificationDeposition a) => FailedVerification(a), () => input);

    private static Flow<Flow> TestMethodInfoDeposition(TestMethodInfoDeposition? input) =>
        Style
            .OnNewLine().Trace($"Test:                    {input!.MethodName}")
            .OnNewLine().Trace($"Location:                {input.SourceFile}:{input.LineNumber}:1");

    private static string Pluralize(int count, string str) =>
        count > 1 ? $"{str}s" : str;

    private static Flow<Flow> Evidence(CaseFile caseFile)
    {
        var originalExecutionCountText = $"{caseFile.OriginalRunExecutionCount} {Pluralize(caseFile.OriginalRunExecutionCount, "execution")}";
        var executionCountText = $"{caseFile.ExecutionCount} {Pluralize(caseFile.ExecutionCount, "execution")}";
        var shrinkCountText = $" (after {caseFile.ShrinkCount} {Pluralize(caseFile.ShrinkCount, "shrink")})";
        return
            Style.DrawTopLine()
                .ToFlowIf(caseFile.TestMethodInfoDeposition != null, TestMethodInfoDeposition, () => caseFile.TestMethodInfoDeposition)
                .OnNewLine().Trace($"Original failing run:    {originalExecutionCountText}")
                .When(caseFile.ShrinkCount > 0,
                    Style
                        .OnNewLine()
                        .Trace($"Minimal failing case:    ")
                        .Trace(executionCountText)
                        .Trace(shrinkCountText))
                .OnNewLine().Trace($"Seed:                    {caseFile.Seed}")
                .ToFlow(Execution, caseFile.CollapseExecutions())
                .ToFlow(Failure, caseFile.FailureDeposition);
    }

    private static Flow<Flow> PassedExpectations(IEnumerable<PassedExpectationDeposition> input) =>
        Pulse
            .ToFlow(a => Style.OnNewLine().Trace($"- {a.Label}: {a.TimesPassed}x"), input)
            .DrawLine();

    public static Flow<Flow> Inquiry(Inquiry input) =>
        Style
            .DrawTopLine()
            .OnNewLine().Trace(input.NumberOfRuns).Space().Pluralize(input.NumberOfRuns, "Run")
            .DrawLine()
            .OnNewLine().Trace("Passed Expectations")
            .ToFlow(PassedExpectations, input.PassedExpectationDepositions);

    public static Flow<Flow> Summary(Findings input) =>
        Style
            .DrawTopLine()
            .OnNewLine().Trace(input.NumberOfRuns).Space().Pluralize(input.NumberOfRuns, "Run")
            .DrawLine()
            .OnNewLine().Trace("Passed Expectations")
            .ToFlow(PassedExpectations, input.PassedExpectationDepositions);

    public static Flow<Flow> CaseFile(CaseFile input)
    {
        var hasPassed = input.PassedExpectationDepositions.Count > 0;
        var breakBeforePassed = input.HasEvidence && hasPassed;
        return Pulse
            .ToFlowIf(input.HasEvidence, Evidence, () => input)
            .When(breakBeforePassed, Style.NewLine())
            .TraceIf(hasPassed, () => " Passed Expectations")
            .ToFlowIf(hasPassed, PassedExpectations, () => input.PassedExpectationDepositions);
    }

    public static Flow<Flow> Flow(IRecord input, Decorum decorum)
    {
        return
            Pulse
                .Prime(() => decorum).Dissipate()
                .Prime(() => new Joiner()).Dissipate()
                .Prime(() => new MaxLabel(0)).Dissipate()
                .OnType((Inquiry a) => Inquiry(a), () => input)
                .OnType((Findings a) => Summary(a), () => input)
                .OnType((CaseFile a) => CaseFile(a), () => input);
    }
}
