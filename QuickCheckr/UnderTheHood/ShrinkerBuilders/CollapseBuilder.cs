using System.Linq.Expressions;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

public class CollapseBuilder<T>
{
    public ICollapseOption[] Using(params CollapseOption<T>[] collapsers) =>
        collapsers;

    public CollapseOption<T> Member<TMember>(
        Expression<Func<T, TMember>> member,
        Func<T, TMember, T> replace) =>
        Collapse.Member(member, replace);
}
