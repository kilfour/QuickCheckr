using System.Linq.Expressions;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;

namespace QuickCheckr.UnderTheHood.ShrinkerBuilders;

public class TraversalBuilder<T>
{
    public ITraversalOption[] Using(params TraversalOption<T>[] traversals) =>
        traversals;

    public TraversalOption<T> Member<TMember>(
        Expression<Func<T, TMember>> member,
        Func<T, TMember, T> replace) =>
        Traversal.Member(member, replace);
}
