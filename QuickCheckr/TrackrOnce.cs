using QuickCheckr.UnderTheHood;

namespace QuickCheckr;


public static partial class Trackr
{
    public static CheckrOf<Case> Once(
        string key,
        CheckrOf<Case> checkr) =>
        async state =>
        {
            var flag = state.Memory.GetLongTermVariable(key, false);
            if (!flag)
            {
                state.Memory.StoreLongTermVariable(key, true);
                return await checkr(state).ConfigureAwait(false);
            }
            return CheckrResult.CaseOnly(state);
        };
}
