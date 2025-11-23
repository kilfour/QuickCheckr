using System.Runtime.CompilerServices;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> OneOf<T>(params CheckrOf<T>[] scripts)
        => OneOfInternal(scripts);

    private static CheckrOf<T> OneOfInternal<T>(
        CheckrOf<T>[] scripts,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "")
    {
        var key = $"OneOf|{Path.GetFileName(file)}:{line}|{member}|{scripts.Length}";
        return state =>
            {
                var index = state.ShortTermMemories.Current.StoreVariable(key,
                    () => Fuzzr.Int(0, scripts.Length)(state.FuzzrState).Value);
                return scripts[index](state);
            };
    }

    // public static CheckrOf<T> OneOfIf<T>(params (Func<bool> condition, CheckrOf<T> script)[] scripts)
    // 	=> OneOfIfInternal(scripts);

    // private static CheckrOf<T> OneOfIfInternal<T>(
    // 	 (Func<bool> condition, CheckrOf<T> script)[] scripts,
    // 	[CallerFilePath] string file = "",
    // 	[CallerLineNumber] int line = 0,
    // 	[CallerMemberName] string member = "")
    // {
    // 	var key = $"OneOf|{Path.GetFileName(file)}:{line}|{member}|{scripts.Length}";
    // 	return state =>
    // 		{
    // 			var validScriptIndexes =
    // 				scripts.Select((a, ix) => (ix, a.condition))
    // 					.Where(a => a.condition())
    // 					.Select(a => a.ix);
    // 			var index = state.CurrentExecutionContext().Remember(key,
    // 				() => Fuzzr.OneOf(validScriptIndexes)(state.FuzzState).Value, ReportingIntent.Never);
    // 			var script = scripts[index];
    // 			if (script.condition())
    // 				return scripts[index].script(state);
    // 			return Vessel.None<T>(state);
    // 		};
    // }
}