using QuickCheckr;
using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.Protocol;

/// <summary>
/// Diagnostic procedures that define how InternalAffair data is collected and traced.  
/// These flows determine what information is emitted when an InternalAffair is enabled.
/// </summary>
public static class Procedure
{
    /// <summary>
    /// Traces all filings by type.  
    /// Use to observe the sequence of internal events produced during a run.
    /// </summary>
    public static readonly Flow<IFiling> Events =
        from filing in Pulse.Start<IFiling>()
        from trace in Pulse.Trace(filing.GetType().Name)
        select filing;

    private static readonly Flow<InputShrinkingClueFound> clueFlow =
        from filing in Pulse.Start<InputShrinkingClueFound>()
        from manOfWealthAndTaste in Pulse.Draw<ManOfWealthAndTaste>()
        from _ in Pulse.Trace(manOfWealthAndTaste.ToPrettyPrint().IntroduceThis(filing.Clue))
        from __ in Pulse.Trace("-------------------------------------")
        select filing;

    /// <summary>
    /// Traces clues detected during input shrinking.  
    /// Use to understand how the shrinker evaluates relevance and removes inputs.
    /// </summary>
    public static readonly Flow<IFiling> InputShrinking =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(clueFlow, () => filing)
        select filing;

    private static readonly Flow<ExecutionRemoved> executionClueFlow =
        from filing in Pulse.Start<ExecutionRemoved>()
        from manOfWealthAndTaste in Pulse.Draw<ManOfWealthAndTaste>()
        from _ in Pulse.Trace($"Removed Execution {filing.ExecutionId}.")
        select filing;

    public static readonly Flow<IFiling> ExecutionShrinking =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(executionClueFlow, () => filing)
        select filing;

    private static readonly Flow<EvidenceFailure> evidenceFailureFlow =
        from filing in Pulse.Start<EvidenceFailure>()
        from manOfWealthAndTaste in Pulse.Draw<ManOfWealthAndTaste>()
        from _1 in Pulse.Trace(filing.Exception.Message)
        from _2 in Pulse.Trace(filing.Exception.StackTrace!)
        from _3 in Pulse.Trace("-------------------------------------")
        select filing;

    /// <summary>
    /// Traces detailed information whenever an exception is recorded.  
    /// Use to investigate what exceptions are being thrown during a run.
    /// </summary>
    public static readonly Flow<IFiling> EvidenceFailures =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(evidenceFailureFlow, () => filing)
        select filing;

    private static readonly Flow<InputShrinkingClueFound> nullIssue =
        from filing in Pulse.Start<InputShrinkingClueFound>()
        from manOfWealthAndTaste in Pulse.Draw<ManOfWealthAndTaste>()
        let relevant = filing.Clue.Strategy == "CantShrinkNull"
        from _ in Pulse.TraceIf(relevant,
            () => manOfWealthAndTaste.ToPrettyPrint().IntroduceThis(filing.Clue))
        from __ in Pulse.TraceIf(relevant, () => "-------------------------------------")
        select filing;

    /// <summary>
    /// Highlights and traces cases where null-related structures prevent input shrinking.  
    /// Use to diagnose shrinking paths blocked by null dependencies.
    /// </summary>
    public static readonly Flow<IFiling> NullIssues =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(nullIssue, () => filing)
        select filing;
}
