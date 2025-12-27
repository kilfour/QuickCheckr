using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> Option(
        Func<bool> condition,
        CheckrOf<Case> checkr) =>
            state => CheckrResult.Some(state, (condition, checkr));
}