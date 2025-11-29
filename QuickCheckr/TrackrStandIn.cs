using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> StandIn<T>(T value) =>
        state => { state.RegisterSubstitute(value); return CheckrResult.CaseOnly(state); };
}