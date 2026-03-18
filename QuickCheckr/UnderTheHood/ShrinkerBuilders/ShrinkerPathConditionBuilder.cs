using System.Linq.Expressions;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

public interface IShrinkerPathConditionBuilder<TRoot, T>
{
    IShrinkerPathConditionBuilder<TRoot, TParent> As<TParent>(Expression<Func<TParent, T>> selector);
    IShrinkerPathConditionBuilder<TRoot, TParentDerived> As<TParentDerived, TParentBase>(Expression<Func<TParentDerived, TParentBase>> selector)
        where TParentDerived : TParentBase;
    IShrinkerConditionBuilder<TRoot> Value(Func<T, bool> predicate);
    public IShrinkerConditionBuilder<TRoot> Target();
}

public class ShrinkerPathConditionBuilder<TRoot, T>(
    ShrinkerConditionBuilder<TRoot> shrinkerConditionBuilder,
    List<string> currentPath) : IShrinkerPathConditionBuilder<TRoot, T>
{
    public IShrinkerPathConditionBuilder<TRoot, TParent> As<TParent>(Expression<Func<TParent, T>> selector)
    {
        currentPath.Add(selector.AsMemberInfo().Name);
        return new ShrinkerPathConditionBuilder<TRoot, TParent>(shrinkerConditionBuilder, currentPath);
    }

    public IShrinkerPathConditionBuilder<TRoot, TParentDerived> As<TParentDerived, TParentBase>(Expression<Func<TParentDerived, TParentBase>> selector) where TParentDerived : TParentBase
    {
        currentPath.Add(selector.AsMemberInfo().Name);
        return new ShrinkerPathConditionBuilder<TRoot, TParentDerived>(shrinkerConditionBuilder, currentPath);
    }

    public IShrinkerConditionBuilder<TRoot> Value(Func<T, bool> predicate)
    {
        if (currentPath.Count == 0)
        {
            shrinkerConditionBuilder.Predicates.Add(a => predicate((T)a.Value!));
            return shrinkerConditionBuilder;
        }
        var path = "." + string.Join(".", currentPath.AsEnumerable().Reverse());
        shrinkerConditionBuilder.Predicates.Add(a => a.Path.EndsWith(path) && predicate(GetValue(a)!));
        return shrinkerConditionBuilder;
    }

    public IShrinkerConditionBuilder<TRoot> Target()
    {
        var path = "." + string.Join(".", currentPath.AsEnumerable().Reverse());
        shrinkerConditionBuilder.Predicates.Add(a => a.Path.EndsWith(path));
        return shrinkerConditionBuilder;
    }

    private T? GetValue(ShrinkingInfo info)
    {
        var local = info;
        for (int i = 0; i < currentPath.Count; i++)
            local = local?.Parent;
        return (T?)local?.Value;
    }
}


