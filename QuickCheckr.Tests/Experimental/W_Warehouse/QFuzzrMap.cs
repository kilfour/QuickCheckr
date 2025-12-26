using QuickFuzzr;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.W_Warehouse;

[CodeExample]
public static class QFuzzr
{
    public static FuzzrOf<IEnumerable<U>> Map<T, U>(IEnumerable<T> source, Func<T, FuzzrOf<U>> func) =>
        state =>
        {
            var values = new List<U>();
            foreach (var element in source)
            {
                values.Add(func(element)(state).Value);
            }
            return new Result<IEnumerable<U>>(values, state);
        };
}