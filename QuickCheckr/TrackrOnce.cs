using QuickCheckr.UnderTheHood;

namespace QuickCheckr;


public static partial class Trackr
{
    public static CheckrOf<Case> Once(
        string key,
        CheckrOf<Case> checkr) =>
        state =>
        {
            var flag = state.Memory.GetLongTermVariable(key, false);
            if (!flag)
            {
                state.Memory.StoreLongTermVariable(key, true);
                return CheckrResult.Some(state, checkr(state).Value);
            }
            return CheckrResult.CaseOnly(state);
        };
}
