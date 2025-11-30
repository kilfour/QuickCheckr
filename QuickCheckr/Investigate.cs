using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse;
using QuickPulse.Arteries;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr;

public static class Investigate
{
    public static readonly Investigation Events =
        new(() => FileLog.Write(), Procedure.Events);

    public static readonly Investigation EvidenceFailures =
        new(() => FileLog.Write(), Procedure.EvidenceFailures);

    public static readonly Investigation InputShrinking =
        new(() => FileLog.Write(), Procedure.InputShrinking);

    public static readonly Investigation NullIssues =
        new(() => FileLog.Write(), Procedure.NullIssues);

    public static readonly Investigation[] All =
        [Events, EvidenceFailures, InputShrinking];
}

public static class Procedure
{
    public static readonly Flow<IFiling> Events =
        from filing in Pulse.Start<IFiling>()
        from trace in Pulse.Trace(filing.GetType().Name)
        select filing;

    private static readonly ManOfWealthAndTaste manOfWealthAndTaste =
        Please.AllowMe().ToInline<List<int>>();

    private static readonly Flow<InputShrinkingClueFound> clueFlow =
        from filing in Pulse.Start<InputShrinkingClueFound>()
        from _ in Pulse.Trace(manOfWealthAndTaste.IntroduceThis(filing.Clue, true))
        from __ in Pulse.Trace("-------------------------------------")
        select filing;

    public static readonly Flow<IFiling> InputShrinking =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(clueFlow, () => filing)
        select filing;


    private static readonly Flow<EvidenceFailure> evidenceFailureFlow =
        from filing in Pulse.Start<EvidenceFailure>()
        from _ in Pulse.Trace(manOfWealthAndTaste.IntroduceThis(filing, true))
        from __ in Pulse.Trace("-------------------------------------")
        select filing;

    public static readonly Flow<IFiling> EvidenceFailures =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(evidenceFailureFlow, () => filing)
        select filing;


    private static readonly Flow<InputShrinkingClueFound> nullIssue =
        from filing in Pulse.Start<InputShrinkingClueFound>()
        let relevant = filing.Clue.Strategy == "CantShrinkNull"
        from _ in Pulse.TraceIf(relevant,
            () => manOfWealthAndTaste.IntroduceThis(filing.Clue, true))
        from __ in Pulse.TraceIf(relevant, () => "-------------------------------------")
        select filing;

    public static readonly Flow<IFiling> NullIssues =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(nullIssue, () => filing)
        select filing;
}