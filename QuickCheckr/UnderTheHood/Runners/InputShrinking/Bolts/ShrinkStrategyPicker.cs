using System.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkStrategyPicker
{
    public static IEnumerable<InputShrinkingClue> Input<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var actualType = typeof(T) == typeof(object) && value != null
            ? value.GetType()
            : typeof(T);

        var normalizedType = Nullable.GetUnderlyingType(actualType) ?? actualType;

        // var customReducer = state.ReducersRegistry.TryGet<T>();
        // if (customReducer != null)
        // {
        //     new CustomReductionStrategy<T>(customReducer).Reduce(state, key, value!);
        //     return;
        // }

        var customShrinker = state.Memory.GetShrinker<T>(executionId, key);
        if (customShrinker != null)
        {
            return new CustomShrinkStrategy<T>(customShrinker).Shrink(state, check, executionId, key, value, fullKey);
        }

        var primitiveKey = PrimitiveShrinkStrategy.PrimitiveValues.Keys.FirstOrDefault(k => k.IsAssignableFrom(normalizedType));
        if (primitiveKey != null)
        {
            return new PrimitiveShrinkStrategy().Shrink(state, check, executionId, key, value, fullKey);
        }

        // if (actualType.IsEnum)
        // {
        //     new EnumShrinkStrategy().Shrink(state, key, value, fullKey);
        //     return;
        // }

        if (typeof(IEnumerable).IsAssignableFrom(actualType))
        {
            return new EnumerableShrinkStrategy().Shrink(state, check, executionId, key, value, fullKey);
        }

        // if (actualType.IsClass)
        // {
        //     CompositeObjectShrinkStrategy.Shrink(state, key, value, fullKey);
        //     return;
        // }

        // state.CurrentExecutionContext().ShrinkTrace(key, ShrinkKind.Unknown, new ShrinkTrace
        // {
        //     ExecutionId = -1,
        //     Key = fullKey,
        //     Name = fullKey.Split(".").Last(),
        //     Original = value,
        //     Result = null,
        //     Intent = ShrinkIntent.Keep,
        //     Strategy = "NoneFound"
        // });
        return [];
        //yield break;
    }
}
