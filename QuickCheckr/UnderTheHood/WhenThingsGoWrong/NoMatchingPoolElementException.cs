using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public sealed class NoMatchingPoolElementException : QuickCheckrException
{
    public NoMatchingPoolElementException(string key, Type valueType)
        : base(BuildMessage(key, valueType))
    {
    }

    public NoMatchingPoolElementException(string key, Type firstValueType, Type secondValueType)
        : base(BuildMessage(key, firstValueType, secondValueType))
    {
    }

    private static string BuildMessage(string key, Type valueType) =>
        $@"Trackr.OneOfPoolWhen could not choose a '{valueType.Name}' from pool '{key}' because no stored value matched the predicate.

OneOfPoolWhen needs at least one matching stored value whenever it runs.
Check the predicate, or use Trackr.PoolWhen when a match is optional.";

    private static string BuildMessage(string key, Type firstValueType, Type secondValueType) =>
        $@"Trackr.OneOfPoolWhen could not choose a matching pair of '{firstValueType.Name}' and '{secondValueType.Name}' from pool '{key}'.

OneOfPoolWhen needs at least one matching pair whenever it runs.
Check the predicate, or use Trackr.PoolWhen when a match is optional.";
}
