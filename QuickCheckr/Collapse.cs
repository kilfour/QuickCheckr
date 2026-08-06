using System.Linq.Expressions;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

namespace QuickCheckr;

public static class Collapse
{
    public static CollapseOption<T> Member<T, TMember>(
        Expression<Func<T, TMember>> member,
        Func<T, TMember, T> replace)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(replace);

        return new MemberCollapseOption<T, TMember>(
            member.AsMemberInfo().Name,
            member.Compile(),
            replace);
    }
}
