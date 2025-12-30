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

    private readonly static Flow<WarningDeposition> warningDeposition =
        from input in Pulse.Start<WarningDeposition>()
        from _ in newLine
        from __ in Pulse.Trace($"   - WARNING: {input.Value}")
        select input;

    private readonly static Flow<TraceDeposition> traceDeposition =
        from input in Pulse.Start<TraceDeposition>()
        from _ in newLine
        from __ in Pulse.Trace($"   - Trace: {input.Label} = {input.Value}")
        select input;

    private readonly static Flow<PoolTraceDeposition> poolTraceDeposition =
        from input in Pulse.Start<PoolTraceDeposition>()
        from _ in newLine
        from __ in Pulse.Trace($"   -  Pool: {input.Label} = {input.Value}")
        select input;

    private readonly static Flow<InputDeposition> inputDeposition =
        from input in Pulse.Start<InputDeposition>()
        from _ in newLine
        let label = input.Label.Trim()
        from __ in Pulse.Trace($"   - Input: {label} = {input.Value}")
        from ___ in Pulse.TraceIf(input.Original is not null,
            () => $"{Environment.NewLine}{new string(' ', label.Length + 12)} = {input.Original}")
        select input;

    private readonly static Flow<ActionDeposition> actionDeposition =
        from input in Pulse.Start<ActionDeposition>()
        from _ in Pulse.When<Decorum>(a => a.Intersperse.Restricted(), separator)
        from __ in Pulse.Trace($" {input.Label.Trim()}")
        select input;

    private readonly static Flow<ExecutionDeposition> executionDeposition =
        from input in Pulse.Start<ExecutionDeposition>()
        from nl in newLine
        from _1 in line
        let colon = input.HasContent() ? ":" : ""
        from _2 in Pulse.Trace($"  Executed{colon}")
        from _3 in Pulse.Scoped<Decorum>(
            a => a with { Intersperse = Valve.Install() },
            Pulse.ToFlow(actionDeposition, input.ActionDepositions))
        from _4 in Pulse.TraceIf(input.Times > 1, () => $" ({input.Times} Times)")
        from _5 in Pulse.ToFlow(poolTraceDeposition, input.PoolTraceDepositions)
        from _6 in Pulse.ToFlow(inputDeposition, input.InputDepositions)
        from _7 in Pulse.ToFlow(traceDeposition, input.TraceDepositions)
        from _8 in Pulse.ToFlow(warningDeposition, input.WarningDepositions)
        select input;

    private readonly static Flow<VerifyingDeposition> verifyingDeposition =
        from input in Pulse.Start<VerifyingDeposition>()
        let text = $"  !!  The Verifier Disagrees: {input.FailedExpectation}"
        let length = text.Length + 2
        from _1 in newLine.Then(DoubleLineOf(length)).Then(newLine)
        from _2 in Pulse.Trace(text).Then(newLine)
        from _3 in DoubleLineOf(length)
        select input;

    private readonly static Flow<FailedExpectationDeposition> failedExpectationDeposition =
        from input in Pulse.Start<FailedExpectationDeposition>()
        let text = $"  !! Expectation Failed: {input.FailedExpectation.Trim()}"
        let length = text.Length + 2
        from _1 in newLine.Then(DoubleLineOf(length)).Then(newLine)
        from _2 in Pulse.Trace(text).Then(newLine)
        from _3 in DoubleLineOf(length)
        select input;

    private readonly static Flow<ExceptionDeposition> exceptionDeposition =
        from input in Pulse.Start<ExceptionDeposition>()
        from _1 in newLine.Then(DoubleLineOf(75)).Then(newLine)
        from _2 in Pulse.Trace($"  !! {input.Message}").Then(newLine)
        from _3 in DoubleLineOf(75)
        select input;

    private readonly static Flow<FailureDeposition> failureDeposition =
        Pulse.Start<FailureDeposition>(input =>
            Pulse.FirstOf(
                (() => input is FailedExpectationDeposition, () => Pulse.ToFlow(failedExpectationDeposition, (FailedExpectationDeposition)input)),
                (() => input is ExceptionDeposition, () => Pulse.ToFlow(exceptionDeposition, (ExceptionDeposition)input)),
                (() => input is VerifyingDeposition, () => Pulse.ToFlow(verifyingDeposition, (VerifyingDeposition)input))));

    private readonly static Flow<TestMethodInfoDeposition?> testMethodInfoDeposition =
        from input in Pulse.Start<TestMethodInfoDeposition>()
        from _1 in Pulse.Trace($" Test:                    {input.MethodName}").Then(newLine)
        from _2 in Pulse.Trace($" Location:                {input.SourceFile}:{input.LineNumber}:1").Then(newLine)
        select input;

    private static string Pluralize(int count, string str) =>
        count > 1 ? $"{str}s" : str;

    private readonly static Flow<Verdict?> verdict =
        from verdict in Pulse.Start<Verdict>()
        from _ in line
        from __ in Pulse.ToFlowIf(verdict.TestMethodInfoDeposition != null, testMethodInfoDeposition, () => verdict.TestMethodInfoDeposition)
        let __1 = $"{verdict.OriginalRunExecutionCount} {Pluralize(verdict.OriginalRunExecutionCount, "execution")}"
        from _1 in Pulse.Trace($" Original failing run:    {__1}")
        from n1 in newLine
        let _21 = $"{verdict.ExecutionCount} {Pluralize(verdict.ExecutionCount, "execution")}"
        let _22 = $"{verdict.ShrinkCount} {Pluralize(verdict.ShrinkCount, "shrink")}"
        from _2 in Pulse.TraceIf(verdict.ShrinkCount > 0, () => $" Minimal failing case:    {_21} (after {_22}){Environment.NewLine}")
        from _3 in Pulse.Trace($" Seed:                    {verdict.Seed}")
        from _4 in Pulse.ToFlow(executionDeposition, verdict.ExecutionDepositions)
        from _5 in Pulse.ToFlow(failureDeposition, verdict.FailureDeposition)
        select verdict;

    private readonly static Flow<IEnumerable<PassedExpectationDeposition>> passedExpectationDepositions =
        from input in Pulse.Start<IEnumerable<PassedExpectationDeposition>>()
        from _ in Pulse.Trace(" Passed Expectations")
        from __ in Pulse.ToFlow(a => Pulse.Trace($"{Environment.NewLine} - {a.Label}: {a.TimesPassed}x"), input)
        from ___ in Pulse.When(input.Any(), newLine.Then(space).Then(LineOf(60)))
        select input;

    public readonly static Flow<CaseSummary> Summary =
        from input in Pulse.Start<CaseSummary>()
        let runs = $" {input.NumberOfRuns} {Pluralize(input.NumberOfRuns, "Run")}{Environment.NewLine}"
        from _ in Pulse.Trace(runs)
        select input;

    public readonly static Flow<CaseFile> CourtStyleGuide =
        from input in Pulse.Start<CaseFile>()
        from _ in Pulse.Prime(() => new Decorum())
        from verdict in Pulse.ToFlowIf(input.Verdict != null, verdict, () => input.Verdict)
        from summary in Pulse.OnType(Summary, () => input)
        let needsBreak = input.Verdict != null && input.PassedExpectationDepositions.Count > 0
        from nl in Pulse.TraceIf(needsBreak, () => Environment.NewLine)
        from __ in Pulse.ToFlowIf(
            input.PassedExpectationDepositions.Count > 0,
            passedExpectationDepositions,
            () => input.PassedExpectationDepositions)
        select input;
}