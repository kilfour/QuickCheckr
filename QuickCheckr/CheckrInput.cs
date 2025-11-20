using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr) =>
        state =>
        {
            return HandleInput(state, key, fuzzr);
        };

    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
    {
        var value = state.ShortTermMemory.StoreInput(key, () => fuzzr(state.FuzzrState).Value);
        return CheckrResult.Some(state, value!);

        // var execution = state.CurrentExecutionContext();
        // switch (state.Shifter.CurrentPhase)
        // {
        // 	case QAcidPhase.ShrinkInputEval:
        // 	case QAcidPhase.ShrinkingExecutions:
        // 	case QAcidPhase.ShrinkingActions:
        // 		return execution.ContainsKey(key) ?
        // 			Vessel.Some(state, execution.Get<T>(key))
        // 			: Vessel.None<T>(state);

        // 	case QAcidPhase.ShrinkingInputs
        // 		when execution.AlreadyTriedToShrink(key):
        // 		{
        // 			var value = generator(state.FuzzState).Value;
        // 			execution.SetIfNotAlreadyThere(key, value);
        // 			return Vessel.Some(state, value);
        // 		}

        // 	case QAcidPhase.ShrinkingInputs
        // 		when !execution.AlreadyTriedToShrink(key):
        // 		{
        // 			if (execution.ContainsKey(key))
        // 			{
        // 				var value = execution.Get<T>(key);
        // 				ShrinkStrategyPicker.Input(state, key, value, key);
        // 				execution.MarkAsTriedToShrink(key);
        // 				return Vessel.Some(state, value);
        // 			}
        // 			return Vessel.None<T>(state); // TODO Test with multiple actions, first one throws
        // 		}

        // 	default:
        // 		{
        // 			var value = generator(state.FuzzState).Value;
        // 			execution.SetIfNotAlreadyThere(key, value);
        // 			return Vessel.Some(state, value);
        // 		}
        // }
    }
}