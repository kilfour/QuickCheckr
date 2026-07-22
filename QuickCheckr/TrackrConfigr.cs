using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

/// <summary>
/// Applies the supplied QuickFuzzr configuration once per run.  
/// Use when generation rules should be shared by all inputs throughout the run.
/// </summary>
public static partial class Trackr
{
    public static CheckrOf<Case> Configr(FuzzrOf<Intent> configr) =>
        Once(GetKeyFor.TrackrConfigr, state =>
        {
            configr(state.FuzzrState);
            return CheckrResult.CaseOnly(state);
        });
}
