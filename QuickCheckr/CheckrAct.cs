using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<TOutput> Act<TOutput>(string key, Func<TOutput> func)
        => TryCatch(key, func);

    public static CheckrOf<Case> Act(string key, Action action)
        => TryCatch(key, () => { action(); return Case.Closed; });

    private static CheckrOf<TOutput> TryCatch<TOutput>(string key, Func<TOutput> func) =>
        state =>
        {
            if (state.EvidenceRecorder.IsReplay && !state.ShortTermMemory.GetActions().Contains(key))
                return CheckrResult.None<TOutput>(state);
            state.ShortTermMemory.StoreAction(key);
            // var execution = state.CurrentExecutionContext();
            // if (state.Shifter.CurrentPhase == QAcidPhase.NormalRun)
            // 	execution.AddActionKey(key);
            // var needsToAct = execution.ContainsActionKey(key);
            // if (needsToAct)
            // {
            try
            {
                return CheckrResult.Some(state, func());
            }
            catch (Exception ex)
            {
                state.EvidenceRecorder.RecordFailure(ex);
                return CheckrResult.None<TOutput>(state);
            }
            //}
            //return CheckrResult.None<TOutput>(state);
        };
}