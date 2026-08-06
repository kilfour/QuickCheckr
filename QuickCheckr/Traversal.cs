using System.Linq.Expressions;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;

namespace QuickCheckr;

public static class Traversal
{
    public static TraversalOption<T> Member<T, TMember>(
        Expression<Func<T, TMember>> member,
        Func<T, TMember, T> replace)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(replace);

        return new MemberTraversalOption<T, TMember>(
            member.AsMemberInfo().Name,
            member.Compile(),
            replace);
    }
}
