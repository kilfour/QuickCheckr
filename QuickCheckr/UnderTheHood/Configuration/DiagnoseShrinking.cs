using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Configuration;


public static partial class Diagnose
{
    private static readonly ManOfWealthAndTaste manOfWealthAndTaste =
        Please.AllowMe().ToInline<List<int>>();

    private static readonly Flow<InputShrinkingClueFound> clueFlow =
        from filing in Pulse.Start<InputShrinkingClueFound>()
        from _ in Pulse.Trace(manOfWealthAndTaste.IntroduceThis(filing.Clue, true))
        from __ in Pulse.Trace("-------------------------------------")
        select filing;

    public static readonly Flow<IFiling> Shrinking =
        from filing in Pulse.Start<IFiling>()
        from traceClue in Pulse.OnType(clueFlow, () => filing)
        select filing;

    public static readonly Flow<IFiling> Raw =
        from filing in Pulse.Start<IFiling>()
        from trace in Pulse.Trace(filing)
        select filing;
}