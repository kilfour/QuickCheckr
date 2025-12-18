using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public class PoolDoesNotExistException(string key)
    : QuickCheckrException(BuildMessage(key))
{
    private static string BuildMessage(string key) =>
@$"A retrieval attempt was made for a 'Pool' of '{key}', but none was found.

Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
";
}