using QuickCheckr.UnderTheHood;
using QuickPulse.Show;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> When(Func<bool> predicate, CheckrOf<Case> checkr) =>
        state => predicate() ? checkr(state) : CheckrResult.CaseOnly(state);
}