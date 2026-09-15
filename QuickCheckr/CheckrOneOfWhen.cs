using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
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
        async state =>
        {
            var checkrs = new (Func<bool> condition, CheckrOf<Case> checkr)[options.Length];
            for (var i = 0; i < options.Length; i++)
            {
                checkrs[i] = (await options[i](state).ConfigureAwait(false)).Value;
                if (state.RunStopped)
                    return CheckrResult.CaseOnly(state);
            }
            var validScriptIndexes =
                checkrs.Select((a, ix) => (ix, a.condition))
                    .Where(a => a.condition())
                    .Select(a => a.ix);
            var index = state.Memory.StoreVariable(key,
                () =>
                {
                    if (!validScriptIndexes.Any())
                        throw new NoCheckrsAvailableException();
                    return Fuzzr.OneOf(validScriptIndexes)(state.FuzzrState).Value;
                });
            var checkr = checkrs[index];
            if (checkr.condition())
                return await checkrs[index].checkr(state).ConfigureAwait(false);
            return CheckrResult.CaseOnly(state);
        };
}
