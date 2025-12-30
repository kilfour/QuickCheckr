using QuickCheckr.UnderTheHood;
using QuickPulse.Show.Bolts;

namespace QuickCheckr;

public static partial class Showr
{
    public static CheckrOf<Case> Use(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Editor = config();
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForInputs(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Editor = config();
            return CheckrResult.CaseOnly(state);
        };
}