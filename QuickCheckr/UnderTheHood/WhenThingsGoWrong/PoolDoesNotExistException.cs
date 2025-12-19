using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public class PoolDoesNotExistException
    : QuickCheckrException
{
    public PoolDoesNotExistException(string type)
        : base(
@$"A retrieval attempt was made for a 'Pool' of '{type}', but none was found.

Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
")
    { }
    public PoolDoesNotExistException(string type, string key)
        : base(
@$"A storage attempt was made for a 'Pool' of '{type}' using key '{key}', but no such pool was found.

Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
")
    { }
}