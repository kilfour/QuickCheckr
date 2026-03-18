using System.Linq.Expressions;
using Microsoft.VisualBasic;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

public interface IShrinkerConditionBuilder<T>
{
    IShrinkerConditionBuilder<T> Value(Func<T, bool> predicate);
    IShrinkerPathConditionBuilder<T, TParent> As<TParent>(Expression<Func<TParent, T>> selector);
    IShrinkerPathConditionBuilder<T, TParentDerived> As<TParentDerived, TParentBase>(Expression<Func<TParentDerived, TParentBase>> selector)
        where TParentDerived : TParentBase;
}

public class ShrinkerConditionBuilder<T>() : IShrinkerConditionBuilder<T>
{
    public List<Func<ShrinkingInfo, bool>> Predicates { get; } = [];
    public IShrinkerConditionBuilder<T> Value(Func<T, bool> predicate)
    {
        Predicates.Add(a => predicate((T)a.Value!));
        return this;
    }

    public IShrinkerPathConditionBuilder<T, TParent> As<TParent>(Expression<Func<TParent, T>> selector)
        => new ShrinkerPathConditionBuilder<T, TParent>(this, [selector.AsMemberInfo().Name]);

    public IShrinkerPathConditionBuilder<T, TParentDerived> As<TParentDerived, TParentBase>(Expression<Func<TParentDerived, TParentBase>> selector) where TParentDerived : TParentBase
        => new ShrinkerPathConditionBuilder<T, TParentDerived>(this, [selector.AsMemberInfo().Name]);

    public Func<ShrinkingInfo, bool> GetCondition()
    {
        if (Predicates.Count == 0) return a => typeof(T).IsAssignableFrom(a.ActualType);
        return a => Predicates.Prepend(a => typeof(T).IsAssignableFrom(a.ActualType)).All(p => p(a));
    }
}


