using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Declares a value used to distinguish one kind of failure from another
    /// while gathering evidence.
    /// </summary>
    public static CheckrOf<Case> Distinguish<T>(Func<T> distinction) =>
        state =>
        {
            state.Memory.StoreDistinction(() => distinction());
            return CheckrResult.CaseOnly(state);
        };
}
