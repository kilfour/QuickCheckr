using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public sealed class EmptyPoolException(string key, Type valueType)
    : QuickCheckrException(BuildMessage(key, valueType))
{
    private static string BuildMessage(string key, Type valueType) =>
        $@"Trackr.OneOfPool could not choose a '{valueType.Name}' from pool '{key}' because the pool is empty.

OneOfPool needs at least one stored value to choose from.
Add a value to the pool before calling OneOfPool, or use a conditional pool operation when the pool may be empty.";
}
