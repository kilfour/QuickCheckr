using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public sealed class EmptyPoolException(string key, Type valueType, string methodName)
    : QuickCheckrException(BuildMessage(key, valueType, methodName))
{
    private static string BuildMessage(string key, Type valueType, string methodName) =>
        $@"Trackr.{methodName} could not choose a '{valueType.Name}' from pool '{key}' because the pool is empty.

Trackr.{methodName} needs at least one stored value to choose from.
Add a value to the pool before calling Trackr.{methodName}, or use a conditional pool operation when the pool may be empty.";
}
