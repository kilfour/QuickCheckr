using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.UnderTheHood.Memories.Structure;

public class OverlayMemoryBank
{
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
    public Stack<MemoryLens> GetCurrentSwappers()
    {
        var arr = swappers.ToArray();
        Array.Reverse(arr);
        return new Stack<MemoryLens>(arr);
    }

    public IDisposable NestedValue(MemoryLens swapper)
    {
        swappers.Push(swapper);
        return new DisposableAction(() => swappers.Pop());
    }

    public IDisposable ScopedSwap(string key, object value)
        => ScopedSwap(swappers, key, value);

    public IDisposable ScopedSwap(Stack<MemoryLens> lenses, string key, object value)
    {
        Func<object> getValue = () => Get<object>(key);
        var setSteps = GetSetSteps(lenses, ref getValue);
        var oldValue = getValue();
        Set(key, SetValue(value, setSteps));
        return new DisposableAction(() => Set(key, SetValue(oldValue, setSteps)));
    }

    private Stack<(MemoryLens lens, object container)> GetSetSteps(
        Stack<MemoryLens> lenses,
        ref Func<object> getValue)
    {
        var setSteps = new Stack<(MemoryLens lens, object container)>();
        foreach (var lens in lenses.Reverse())
        {
            var currentGet = getValue;
            getValue = () => lens.Get(currentGet());
            var container = currentGet();
            setSteps.Push((lens, container));
        }
        return setSteps;
    }

    private static object SetValue(object newLeaf, Stack<(MemoryLens lens, object container)> setSteps)
    {
        var updated = newLeaf;
        foreach (var (lens, container) in setSteps)
            updated = lens.Set(container, updated);
        return updated;
    }

    public void Commit(string key, object value)
    {
        Func<object> getValue = () => Get<object>(key);
        var setSteps = GetSetSteps(swappers, ref getValue);
        Set(key, SetValue(value, setSteps));
    }
}