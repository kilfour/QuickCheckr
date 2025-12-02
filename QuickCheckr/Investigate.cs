using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse;
using QuickPulse.Arteries;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr;
/// <summary>
/// Predefined investigation modes that enable additional diagnostic output during a run.  
/// Use these to gain insight into events, failures, or shrinking behaviour while debugging a Checkr.
/// </summary>
public static class Investigate
{
    /// <summary>
    /// Records internal events emitted during execution.  
    /// Use when you want a high-level view of what the engine is doing.
    /// </summary>
    public static readonly Investigation Events =
        new(() => FileLog.Write(), Procedure.Events);

    /// <summary>
    /// Records detailed information whenever an exception is recorded.  
    /// Use to investigate what exceptions are being thrown during a run.
    /// </summary>
    public static readonly Investigation EvidenceFailures =
        new(() => FileLog.Write(), Procedure.EvidenceFailures);

    /// <summary>
    /// Records clues produced during input shrinking.  
    /// Use when debugging why a particular minimal case was chosen.
    /// </summary>
    public static readonly Investigation InputShrinking =
        new(() => FileLog.Write(), Procedure.InputShrinking);

    /// <summary>
    /// Highlights cases where null values interfere with shrinking.  
    /// Use to identify problematic null structures that prevent full minimisation.
    /// </summary>
    public static readonly Investigation NullIssues =
        new(() => FileLog.Write(), Procedure.NullIssues);

    /// <summary>
    /// A convenience collection of commonly useful investigations.  
    /// Use to quickly enable broad diagnostics during development.
    /// </summary>
    public static readonly Investigation[] All =
        [Events, EvidenceFailures, InputShrinking];
}

/// <summary>
/// Diagnostic procedures that define how investigation data is collected and traced.  
/// These flows determine what information is emitted when an investigation is enabled.
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

    private static readonly Flow<EvidenceFailure> evidenceFailureFlow =
        from filing in Pulse.Start<EvidenceFailure>()
        from manOfWealthAndTaste in Pulse.Draw<ManOfWealthAndTaste>()
        from _ in Pulse.Trace(manOfWealthAndTaste.ToPrettyPrint().IntroduceThis(filing))
        from __ in Pulse.Trace("-------------------------------------")
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
