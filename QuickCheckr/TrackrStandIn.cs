using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Registers a stand-in value that shrinking may use to replace elements instead of removing them.  
    /// Use when repeated or dependent values would otherwise cause greedy shrinking,
    /// allowing the shrinker to rewrite candidates to a stable neutral value instead of removing structure.
    /// </summary>
    // public static CheckrOf<Case> StandIn<T>(T value) =>
    //     state => { state.RegisterSubstitute(value); return CheckrResult.CaseOnly(state); };

}