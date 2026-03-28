using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public record Valve
{
    private bool open = false;
    private Valve(bool open) { this.open = open; }


    public static Valve Closed() => new(false);


    public static Valve Install() => new(true);


    public Valve Open() { open = true; return this; }


    public bool Passable() { var result = open; open = false; return result; }


    public bool Restricted() => !Passable();
}

public record Decorum
{
    public Valve Intersperse { get; init; } = Valve.Closed();
    public Valve Line { get; init; } = Valve.Closed();
}

public record MaxLabel(int Length);

public static class The
{
    private readonly static Flow<Flow> separator = Pulse.Trace(",");

    private readonly static Flow<Flow> space = Pulse.Trace(" ");

    private static readonly Flow<Flow> newLine = Pulse.Trace(Environment.NewLine);

    private static Flow<Flow> LineOf(int length) => Pulse.Trace(new string('-', length));

    private static Flow<Flow> DoubleLineOf(int length) =>
        space.Then(Pulse.Trace(new string('=', length)));

    private readonly static Flow<Flow> line =
        space.Then(LineOf(60)).Then(newLine);

    private static Flow<Flow> Warning(WarningDeposition input) =>
        newLine.Then(Pulse.Trace($"   - WARNING: {input.Value}"));

    private static Flow<Flow> Trace(TraceDeposition input) =>
        from maxLabel in Pulse.Draw<MaxLabel>()
        from _1 in newLine
        from _2 in Pulse.Trace("   - ")
        from _3 in Pulse.TraceIf(input.Labeled, () => "Trace: ")
        from _4 in Pulse.Trace($"{input.Label.PadRight(maxLabel.Length, ' ')} = {input.Value}")
        select Flow.Continue;

    private static Flow<Flow> PoolTrace(PoolTraceDeposition input) =>
        from maxLabel in Pulse.Draw<MaxLabel>()
        from _ in newLine
        from __ in Pulse.Trace("   - ")
        from ___ in Pulse.TraceIf(input.Labeled, () => " Pool: ")
        from ____ in Pulse.Trace($"{input.Label.PadRight(maxLabel.Length, ' ')} = {input.Value}")
        select Flow.Continue;

    private static Flow<Flow> Redux(InputDeposition input) =>
        from maxLabel in Pulse.Draw<MaxLabel>()
        let spaces = input.Labeled ? 12 : 5
        from _1 in newLine
        from _2 in Pulse.Trace(new string(' ', spaces))
        from _3 in Pulse.Trace("Redux")
        from _4 in Pulse.Trace(new string(' ', Math.Max(3, maxLabel.Length - 2)))
        from _5 in Pulse.Trace(input.Redux.Value!)
        select Flow.Continue;

    private static Flow<Flow> Input(InputDeposition input) =>
        from _1 in newLine
        from maxLabel in Pulse.Draw<MaxLabel>()
        from _2 in Pulse.Trace("   - ")
        from _3 in Pulse.TraceIf(input.Labeled, () => "Input: ")
        from _4 in Pulse.Trace($"{input.Label.PadRight(maxLabel.Length, ' ')} = {input.Value}")
        let spaces = input.Labeled ? 15 : 8
        from _5 in Pulse.TraceIf(input.Original.HasValue,
            () => $"{Environment.NewLine}{new string(' ', maxLabel.Length + spaces)}{input.Original.Value}")
        from _6 in Pulse.ToFlowIf(input.Redux.HasValue, Redux, () => input)
        select Flow.Continue;

    private static Flow<Flow> Action(ActionDeposition input) =>
        from _ in Pulse.When<Decorum>(a => a.Intersperse.Restricted(), separator)
        from __ in Pulse.Trace($" {input.Label}")
        select Flow.Continue;

    private static Flow<Flow> Execution(CollapsableExecutionDeposition input) =>

        from nl in newLine
        from _1 in line
        let execution = input.ExecutionDeposition
        let colon = execution.HasContent() ? ":" : ""
        from labelLength in Pulse.Manipulate<MaxLabel>(a => a with { Length = execution.MaxLabelLength() })
        from _2 in Pulse.Trace($"  Executed{colon}")
        from _3 in Pulse.Scoped<Decorum>(
            a => a with { Intersperse = Valve.Install() },
            Pulse.ToFlow(Action, execution.ActionDepositions))
        from _4 in Pulse.TraceIf(input.Times > 1, () => $" ({input.Times} Times)")
        from _5 in Pulse.ToFlow(PoolTrace, execution.PoolTraceDepositions)
        from _6 in Pulse.ToFlow(Input, execution.InputDepositions)
        from _7 in Pulse.ToFlow(Trace, execution.TraceDepositions)
        from _8 in Pulse.ToFlow(Warning, execution.GetWarningDepositionsForReport())
        select Flow.Continue;

    private static Flow<Flow> FailedVerification(FailedVerificationDeposition input) =>
        from _ in Pulse.NoOp()
        let text = $"  !!  The Verifier Disagrees: {input.FailedExpectation}"
        let length = text.Length + 2
        from _1 in newLine.Then(DoubleLineOf(length)).Then(newLine)
        from _2 in Pulse.Trace(text).Then(newLine)
        from _3 in DoubleLineOf(length)
        select Flow.Continue;

    private static Flow<Flow> FailedExpectation(FailedExpectationDeposition input) =>
        from _ in Pulse.NoOp()
        let text = $"  !! Expectation Failed: {input.FailedExpectation}"
        let length = text.Length + 2
        from _1 in newLine.Then(DoubleLineOf(length)).Then(newLine)
        from _2 in Pulse.Trace(text).Then(newLine)
        from _3 in DoubleLineOf(length)
        select Flow.Continue;

    private static Flow<Flow> Exception(FailedExceptionDeposition input) =>
        from _1 in newLine.Then(DoubleLineOf(75)).Then(newLine)
        from _2 in Pulse.Trace($"  !! {input.Message}").Then(newLine)
        from _3 in DoubleLineOf(75)
        select Flow.Continue;

    private static Flow<Flow> Failure(FailureDeposition input) =>
        from _1 in Pulse.OnType((FailedExpectationDeposition a) => FailedExpectation(a), () => input)
        from _2 in Pulse.OnType((FailedExceptionDeposition a) => Exception(a), () => input)
        from _3 in Pulse.OnType((FailedVerificationDeposition a) => FailedVerification(a), () => input)
        select Flow.Continue;

    private static Flow<Flow> TestMethodInfoDeposition(TestMethodInfoDeposition? input) =>
        from _1 in Pulse.Trace($" Test:                    {input!.MethodName}").Then(newLine)
        from _2 in Pulse.Trace($" Location:                {input.SourceFile}:{input.LineNumber}:1").Then(newLine)
        select Flow.Continue;

    private static string Pluralize(int count, string str) =>
        count > 1 ? $"{str}s" : str;

    private static Flow<Flow> Evidence(CaseFile caseFile) =>
        from _ in line
        from __ in Pulse.ToFlowIf(caseFile.TestMethodInfoDeposition != null, TestMethodInfoDeposition, () => caseFile.TestMethodInfoDeposition)
        let __1 = $"{caseFile.OriginalRunExecutionCount} {Pluralize(caseFile.OriginalRunExecutionCount, "execution")}"
        from _1 in Pulse.Trace($" Original failing run:    {__1}")
        from n1 in newLine
        let _21 = $"{caseFile.ExecutionCount} {Pluralize(caseFile.ExecutionCount, "execution")}"
        let _22 = $"{caseFile.ShrinkCount} {Pluralize(caseFile.ShrinkCount, "shrink")}"
        from _2 in Pulse.TraceIf(caseFile.ShrinkCount > 0, () => $" Minimal failing case:    {_21} (after {_22}){Environment.NewLine}")
        from _3 in Pulse.Trace($" Seed:                    {caseFile.Seed}")
        from _4 in Pulse.ToFlow(Execution, caseFile.CollapseExecutions())
        from _5 in Pulse.ToFlow(Failure, caseFile.FailureDeposition)
        select Flow.Continue;

    private static Flow<Flow> UncheckedExpectationDepositions(IEnumerable<UncheckedExpectationDeposition> input) =>
        from _ in Pulse.Trace(" ! Warning: Unchecked Expectations !")
        from __ in Pulse.ToFlow(a => Pulse.Trace($"{Environment.NewLine} - {a.Label}"), input)
        from ___ in Pulse.When(input.Any(), newLine.Then(space).Then(LineOf(60)))
        select Flow.Continue;

    private static Flow<Flow> PassedExpectationDepositions(IEnumerable<PassedExpectationDeposition> input) =>
        from _ in Pulse.Trace(" Passed Expectations")
        from __ in Pulse.ToFlow(a => Pulse.Trace($"{Environment.NewLine} - {a.Label}: {a.TimesPassed}x"), input)
        from ___ in Pulse.When(input.Any(), newLine.Then(space).Then(LineOf(60)))
        select Flow.Continue;

    public static Flow<Flow> Summary(CaseSummary input) =>
        Pulse.Trace($" {input.NumberOfRuns} {Pluralize(input.NumberOfRuns, "Run")}{Environment.NewLine}");

    public static Flow<Flow> CourtStyleGuide(CaseFile input) =>
        from _1 in Pulse.Prime(() => new Decorum())
        from _2 in Pulse.Prime(() => new MaxLabel(0))
        from _3 in Pulse.ToFlowIf(input.HasEvidence, Evidence, () => input)
        from summary in Pulse.OnType((CaseSummary a) => Summary(a), () => input)
        let needsBreak = input.HasEvidence && input.UncheckedExpectationDepositions.Count > 0
        from nl in Pulse.TraceIf(needsBreak, () => Environment.NewLine)
        from _ in Pulse.ToFlowIf(
            input.UncheckedExpectationDepositions.Count > 0,
            UncheckedExpectationDepositions,
            () => input.UncheckedExpectationDepositions)
        let needsBreak_ =
            (input.HasEvidence || input.UncheckedExpectationDepositions.Count > 0)
            && input.PassedExpectationDepositions.Count > 0
        from nl_ in Pulse.TraceIf(needsBreak_, () => Environment.NewLine)
        from __ in Pulse.ToFlowIf(
            input.PassedExpectationDepositions.Count > 0,
            PassedExpectationDepositions,
            () => input.PassedExpectationDepositions)
        select Flow.Continue;
}