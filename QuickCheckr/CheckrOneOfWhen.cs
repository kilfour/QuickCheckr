using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Chooses one of the provided Checkr scripts whose condition evaluates to true, and executes it as the next step.  
    /// Use when modelling behaviour where only certain actions are valid in the current state.
    /// </summary>
    public static CheckrOf<Case> OneOfWhen(params CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)>[] options)
        => OneOfWhen(GetKeyFor.CheckrOneOfWhen, options);

    public static CheckrOf<Case> OneOfWhen(
        string key,
        params CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)>[] options) =>
        state =>
        {
            var checkrs = options.Select(a => a(state).Value).ToArray();
            var validScriptIndexes =
                checkrs.Select((a, ix) => (ix, a.condition))
                    .Where(a => a.condition())
                    .Select(a => a.ix);
            var index = state.Memory.StoreVariable(key,
                () => Fuzzr.OneOf(validScriptIndexes)(state.FuzzrState).Value);
            var checkr = checkrs[index];
            if (checkr.condition())
                return checkrs[index].checkr(state);
            return CheckrResult.CaseOnly(state);
        };
}