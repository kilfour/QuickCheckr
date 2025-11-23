namespace QuickCheckr.UnderTheHood.Memories;

public class OverlayMemoryBank : MemoryBank
{
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