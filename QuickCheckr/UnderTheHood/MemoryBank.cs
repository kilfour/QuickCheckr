namespace QuickCheckr.UnderTheHood;

public class MemoryBank
{
    private readonly Dictionary<string, object?> values = [];
    public IReadOnlyDictionary<string, object?> Values => values;
    public T? Store<T>(string key, Func<T> factory)
    {
        if (!values.TryGetValue(key, out object? value))
        {
            value = factory();
            values[key] = value;
        }
        var val = (T?)value;
        return val;
    }
    //public T Get<T>(string key) => (T)values[key]!;
}