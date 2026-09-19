using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public sealed record CheckrOption(Func<bool> Condition, CheckrOf<Case> Checkr);

public static partial class Checkr
{
    public static CheckrOf<CheckrOption> Option(
        Func<bool> condition,
        CheckrOf<Case> checkr) =>
            state => CheckrResult.Some(state, new CheckrOption(condition, checkr));
}