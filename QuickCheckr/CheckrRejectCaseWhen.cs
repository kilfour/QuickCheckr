using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Excludes the failing case from selection when the predicate is true.
    /// The predicate is evaluated after shrinking during the case-selection replay.
    /// </summary>
    public static CheckrOf<Case> RejectCaseWhen(Func<bool> predicate) =>
        state =>
        {
            state.Memory.StoreCaseRejection(predicate);
            return CheckrResult.CaseOnly(state);
        };
}
