using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class The
{
    public record MaxLabel(int Length);

    private readonly static Flow<Flow> separator = Pulse.Trace(",");

    private readonly static Flow<Flow> space = Pulse.Trace(" ");

    private static readonly Flow<Flow> newLine = Pulse.Trace(Environment.NewLine);
    private static Flow<Flow> NewLine(this Flow<Flow> other)
        => other.Then(newLine);

    private static Flow<Flow> LineOf(int length) => Pulse.Trace(new string('-', length));

    private static Flow<Flow> DoubleLineOf(int length) =>
        space.Then(Pulse.Trace(new string('=', length)));

    private static Flow<Flow> DoubleLineOf(this Flow<Flow> other, int length) =>
        other.Then(DoubleLineOf(length));

    private readonly static Flow<Flow> line =
        space.Then(LineOf(60));
    private static Flow<Flow> Line(this Flow<Flow> other)
        => other.Then(line);

    private static Flow<Flow> Warning(WarningDeposition input) =>
        newLine.Then(Pulse.Trace($"   - WARNING: {input.Value}"));

    private static Flow<Flow> Labeled(bool labeled, string type, string label, string value) =>
        from maxLabel in Pulse.Draw<MaxLabel>()
        from _ in
            newLine
                .Trace("   - ")
                .TraceIf(labeled, () => $"{type}: ")
                .Trace($"{label.PadRight(maxLabel.Length, ' ')} = {value}")
        select Flow.Continue;

    private static Flow<Flow> Trace(TraceDeposition input) =>
        Labeled(input.Labeled, "Trace", input.Label, input.Value);

    private static Flow<Flow> FinalTrace(TraceDeposition input) =>
        Labeled(input.Labeled, "Final", input.Label, input.Value);

    private static Flow<Flow> PoolTrace(PoolTraceDeposition input) =>
        Labeled(input.Labeled, " Pool", input.Label, input.Value);

    private static Flow<Flow> Redux(InputDeposition input)
    {
        var leftPadding = input.Labeled ? 12 : 5;
        return
            from maxLabel in Pulse.Draw<MaxLabel>()
            from _ in newLine
                .Trace(new string(' ', leftPadding))
                .Trace("Redux")
                .Trace(new string(' ', Math.Max(3, maxLabel.Length - 2)))
                .Trace(input.Redux.Value!)
            select Flow.Continue;
    }

    private static Flow<Flow> Input(InputDeposition input)
    {
        var spaces = input.Labeled ? 15 : 8;
        return
            from maxLabel in Pulse.Draw<MaxLabel>()
            from _ in
                newLine
                    .Trace("   - ")
                    .TraceIf(input.Labeled, () => "Input: ")
                    .Trace($"{input.Label.PadRight(maxLabel.Length, ' ')} = {input.Value}")
                    .When(input.Original.HasValue,
                        newLine
                            .Trace(new string(' ', maxLabel.Length + spaces))
                            .Trace(input.Original.Value!))
                    .ToFlowIf(input.Redux.HasValue, Redux, () => input)
            select Flow.Continue;
    }

    private static Flow<Flow> Action(ActionDeposition input) =>
        Pulse
            .When<Joiner>(a => a.NeedsSeparator(), separator)
            .Trace($" {input.Label}");

    private static Flow<Flow> Actions(IEnumerable<ActionDeposition> input) =>
        Pulse.Scoped<Joiner>(a => a.Start(),
            Pulse.ToFlow(Action, input));

    private static Flow<Flow> Actions(this Flow<Flow> other, IEnumerable<ActionDeposition> input) =>
        other.Then(Actions(input));

    private static Flow<Flow> Execution(CollapsableExecutionDeposition input, ExecutionDeposition execution) =>
        Pulse
            .Manipulate<MaxLabel>(a => a with { Length = execution.MaxLabelLength() }).Dissipate()
            .NewLine()
            .Line()
            .NewLine()
            .Trace($"  Executed")
            .TraceIf(execution.HasContent(), () => ":")
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
        var text = $"  !! The Verifier Disagrees: {input.FailedExpectation}";
        var length = text.Length + 2;
        return newLine
            .DoubleLineOf(length)
            .NewLine()
            .Trace(text)
            .NewLine()
            .DoubleLineOf(length);
    }

    private static Flow<Flow> FailedExpectationMessage(string message) =>
        Pulse.Trace($"       {message}").Then(newLine);

    private static Flow<Flow> FailedExpectation(FailedExpectationDeposition input)
    {
        var text = $"  !! Expectation Failed: {input.FailedExpectation}";
        var length = text.Length + 2;
        return newLine
            .DoubleLineOf(length)
            .NewLine()
            .Trace(text)
            .NewLine()
            .ToFlow(FailedExpectationMessage, input.Messages)
            .DoubleLineOf(length);
    }

    private static Flow<Flow> Exception(FailedExceptionDeposition input) =>
        newLine
            .DoubleLineOf(75)
            .NewLine()
            .Trace($"  !! {input.Message}")
            .NewLine()
            .DoubleLineOf(75);

    private static Flow<Flow> Failure(FailureDeposition input) =>
        Pulse
            .OnType((FailedExpectationDeposition a) => FailedExpectation(a), () => input)
            .OnType((FailedExceptionDeposition a) => Exception(a), () => input)
            .OnType((FailedVerificationDeposition a) => FailedVerification(a), () => input);

    private static Flow<Flow> TestMethodInfoDeposition(TestMethodInfoDeposition? input) =>
        Pulse
            .Trace($" Test:                    {input!.MethodName}").Then(newLine)
            .Trace($" Location:                {input.SourceFile}:{input.LineNumber}:1").Then(newLine);

    private static string Pluralize(int count, string str) =>
        count > 1 ? $"{str}s" : str;

    private static Flow<Flow> Evidence(CaseFile caseFile)
    {
        var originalExecutionCountText = $"{caseFile.OriginalRunExecutionCount} {Pluralize(caseFile.OriginalRunExecutionCount, "execution")}";
        var executionCountText = $"{caseFile.ExecutionCount} {Pluralize(caseFile.ExecutionCount, "execution")}";
        var shrinkCountText = $" (after {caseFile.ShrinkCount} {Pluralize(caseFile.ShrinkCount, "shrink")})";
        return
            line
                .NewLine()
                .ToFlowIf(caseFile.TestMethodInfoDeposition != null, TestMethodInfoDeposition, () => caseFile.TestMethodInfoDeposition)
                .Trace($" Original failing run:    {originalExecutionCountText}")
                .NewLine()
                .When(caseFile.ShrinkCount > 0,
                    Pulse
                        .Trace($" Minimal failing case:    ")
                        .Trace(executionCountText)
                        .Trace(shrinkCountText)
                        .NewLine())
                .Trace($" Seed:                    {caseFile.Seed}")
                .ToFlow(Execution, caseFile.CollapseExecutions())
                .ToFlow(Failure, caseFile.FailureDeposition);
    }

    private static Flow<Flow> UncheckedExpectations(IEnumerable<UncheckedExpectationDeposition> input) =>
        Pulse
            .Trace(" ! Warning: Unchecked Expectations !")
            .ToFlow(a => Pulse.Trace($"{Environment.NewLine} - {a.Label}"), input)
            .When(input.Any(), newLine.Line());

    private static Flow<Flow> PassedExpectations(IEnumerable<PassedExpectationDeposition> input) =>
        Pulse
            .Trace(" Passed Expectations")
            .ToFlow(a => newLine.Trace($" - {a.Label}: {a.TimesPassed}x"), input)
            .When(input.Any(), newLine.Line());

    public static Flow<Flow> Summary(CaseSummary input) =>
        Pulse.Trace($" {input.NumberOfRuns} {Pluralize(input.NumberOfRuns, "Run")}{Environment.NewLine}");

    public static Flow<Flow> CourtStyleGuide(CaseFile input)
    {
        var hasUnchecked = input.UncheckedExpectationDepositions.Count > 0;
        var breakBeforeUnchecked = input.HasEvidence && hasUnchecked;
        var hasPassed = input.PassedExpectationDepositions.Count > 0;
        var breakBeforePassed = (input.HasEvidence || hasUnchecked) && hasPassed;
        return
            Pulse
                .Prime(() => new Joiner()).Dissipate()
                .Prime(() => new MaxLabel(0)).Dissipate()
                .ToFlowIf(input.HasEvidence, Evidence, () => input)
                .OnType((CaseSummary a) => Summary(a), () => input)
                .When(breakBeforeUnchecked, newLine)
                .ToFlowIf(hasUnchecked, UncheckedExpectations, () => input.UncheckedExpectationDepositions)
                .When(breakBeforePassed, newLine)
                .ToFlowIf(hasPassed, PassedExpectations, () => input.PassedExpectationDepositions);
    }
}