using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.Code;

public static partial class Codr
{

    public static CheckrOf<Case> Arrange(string key, Func<string> code) =>
        state =>
        {
            state.Memory.StoreArrange(key, () => code());
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Act(string key, Func<string> code) =>
        state =>
        {
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Assert(string key, Func<string> code) =>
        state =>
        {
            return CheckrResult.CaseOnly(state);
        };
}