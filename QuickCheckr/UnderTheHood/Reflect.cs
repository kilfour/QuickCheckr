using System.Linq.Expressions;
using System.Reflection;

namespace QuickCheckr.UnderTheHood;

internal static class Reflect
{
    public static MemberInfo AsMemberInfo<TTarget, TMember>(this Expression<Func<TTarget, TMember>> expression)
    {
        if (expression.Body is MemberExpression memberExpr)
            return memberExpr.Member;
        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            return unaryMember.Member;
        throw new ArgumentException($"Expression '{expression}' does not refer to a field or property.");
    }
}
