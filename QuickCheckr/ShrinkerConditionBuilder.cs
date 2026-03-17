using System.Linq.Expressions;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr;

public interface IShrinkerConditionBuilder<T>
{
    void Value(Func<T, bool> predicate);
    IShrinkerConditionBuilder<TParent> As<TParent>(Expression<Func<TParent, T>> selector);
    IShrinkerConditionBuilder<TParentDerived> As<TParentDerived, TParentBase>(Expression<Func<TParentDerived, TParentBase>> selector)
        where TParentDerived : TParentBase;
}

// public class ShrinkerPathConditionBuilder<TRoot, T>(
//     ShrinkerConditionBuilder<TRoot> shrinkerConditionBuilder) : IShrinkerConditionBuilder<TRoot>
// {
//     public IShrinkerConditionBuilder<TRoot> Value(Func<T, bool> predicate)
//     {
//         return shrinkerConditionBuilder;
//     }

//     public IShrinkerConditionBuilder<TParent> Parent<TParent>()
//     {
//         throw new NotImplementedException();
//     }

//     public IShrinkerConditionBuilder<TRoot> Value(Func<TRoot, bool> predicate)
//     {
//         throw new NotImplementedException();
//     }
// }

public class ShrinkerConditionPayload
{
    public readonly List<Func<ShrinkingInfo, bool>> Predicates = [];
    public List<string> CurrentPath = [];
}

public class ShrinkerConditionBuilder<T>(ShrinkerConditionPayload payload = null!) : IShrinkerConditionBuilder<T>
{
    private readonly ShrinkerConditionPayload payload = payload ?? new ShrinkerConditionPayload();

    public void Value(Func<T, bool> predicate)
    {
        if (payload.CurrentPath.Count == 0)
        {
            payload.Predicates.Add(a => predicate((T)a.Value!));
            return;
        }
        var path = "." + string.Join(".", payload.CurrentPath.AsEnumerable().Reverse());
        payload.Predicates.Add(a => a.Path.EndsWith(path) && predicate(GetValue(a)!));
    }

    private T? GetValue(ShrinkingInfo info)
    {
        var local = info;
        for (int i = 0; i < payload.CurrentPath.Count; i++)
            local = local?.Parent;
        return (T?)local?.Value;

    }

    public IShrinkerConditionBuilder<TParent> As<TParent>(Expression<Func<TParent, T>> selector)
    {
        payload.CurrentPath.Add(selector.AsMemberInfo().Name);
        return new ShrinkerConditionBuilder<TParent>(payload);
    }

    public IShrinkerConditionBuilder<TParentDerived> As<TParentDerived, TParentBase>(Expression<Func<TParentDerived, TParentBase>> selector) where TParentDerived : TParentBase
    {
        payload.CurrentPath.Add(selector.AsMemberInfo().Name);
        return new ShrinkerConditionBuilder<TParentDerived>(payload);
    }

    public Func<ShrinkingInfo, bool> GetCondition()
    {
        if (payload.Predicates.Count == 0) return a => typeof(T).IsAssignableFrom(a.ActualType);
        return a => payload.Predicates.Prepend(a => typeof(T).IsAssignableFrom(a.ActualType)).All(p => p(a));
    }
}


