using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class OverlayMemoryBank
{
    public record TypedValue(object? Value, Type Type);

    private readonly Dictionary<string, TypedValue> values = [];
    public IReadOnlyDictionary<string, TypedValue> Values => values;
    public T? Store<T>(string key, Func<T> factory)
    {
        if (!values.TryGetValue(key, out var value))
        {
            value = new TypedValue(factory(), typeof(T));
            values[key] = value;
        }
        var val = (T?)value.Value;
        return val;
    }

    public Type GetActualType(string key) => values[key].Type;

    public T Get<T>(string key)
    {
        values.TryGetValue(key, out var value);
        return (T)value?.Value!;
    }
    public void Set(string key, object? value)
    {
        var typedValue = values[key];
        values[key] = new TypedValue(value, typedValue.Type);
    }

    private readonly Stack<MemoryLens> swappers = new();

    public IDisposable NestedValue(MemoryLens swapper)
    {
        swappers.Push(swapper);
        return new DisposableAction(() => swappers.Pop());
    }

    public IDisposable ScopedSwap(string key, object value)
    {
        Func<object> getValue = () => Get<object>(key);
        var setSteps = new Stack<(MemoryLens lens, object container)>();
        foreach (var lens in swappers.Reverse())
        {
            var currentGet = getValue;
            getValue = () => lens.Get(currentGet());
            var container = currentGet();
            setSteps.Push((lens, container));
        }
        var oldValue = getValue();
        object setValue(object newLeaf)
        {
            var updated = newLeaf;
            foreach (var (lens, container) in setSteps)
                updated = lens.Set(container, updated);
            return updated;
        }
        Set(key, setValue(value));
        return new DisposableAction(() => Set(key, setValue(oldValue)));
    }
}