namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class TypedMemoryBank<T>
{
    private readonly Dictionary<string, T> values = [];
    public IReadOnlyDictionary<string, T> Values => values;
    public T Store(string key, Func<T> factory)
    {
        if (!values.TryGetValue(key, out T? value))
        {
            value = factory();
            values[key] = value;
        }
        var val = value;
        return val;
    }
    public T? Get(string key)
    {
        values.TryGetValue(key, out T? value);
        return value;
    }
}
