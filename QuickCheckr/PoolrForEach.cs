using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>
    /// Runs a Checkr for each pool value. Expectations with the same label are reported once,
    /// with the pool identifiers of failing elements included in their messages.
    /// </summary>
    public static CheckrOf<Case> ForEach<T>(Func<T, CheckrOf<Case>> checkr)
    {
        ArgumentNullException.ThrowIfNull(checkr);
        return ForEachElement<T>((_, value) => checkr(value));
    }

    public static CheckrOf<Case> ForEach<T>(Func<T, bool> predicate, Func<T, CheckrOf<Case>> checkr)
    {
        ArgumentNullException.ThrowIfNull(checkr);
        return ForEachElement<T>((id, value) =>
            predicate(value) ? checkr(value) : s => CheckrResult.CaseOnly(s));
    }

    public static CheckrOf<Case> ForEach<T>(Func<T, PoolEntryOperations<T>, CheckrOf<Case>> checkr)
    {
        ArgumentNullException.ThrowIfNull(checkr);
        return ForEachElement<T>((id, value) => checkr(value, new PoolEntryOperations<T>(id)));
    }

    public static CheckrOf<Case> ForEach<T>(Func<T, bool> predicate, Func<T, PoolEntryOperations<T>, CheckrOf<Case>> checkr)
    {
        ArgumentNullException.ThrowIfNull(checkr);
        return ForEachElement<T>((id, value) =>
            predicate(value) ? checkr(value, new PoolEntryOperations<T>(id)) : s => CheckrResult.CaseOnly(s));
    }
}
