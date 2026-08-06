namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

public interface ICollapseOption
{
    Type TargetType { get; }
    string MemberName { get; }
    Type MemberType { get; }
    object? Get(object target);
    object Replace(object target, object? member);
}

public abstract record CollapseOption<T> : ICollapseOption
{
    public Type TargetType => typeof(T);
    public abstract string MemberName { get; }
    public abstract Type MemberType { get; }
    public abstract object? Get(object target);
    public abstract object Replace(object target, object? member);
}

public sealed record MemberCollapseOption<T, TMember>(
    string Name,
    Func<T, TMember> Getter,
    Func<T, TMember, T> Replacer)
    : CollapseOption<T>
{
    public override string MemberName => Name;
    public override Type MemberType => typeof(TMember);
    public override object? Get(object target) => Getter((T)target);
    public override object Replace(object target, object? member) =>
        Replacer((T)target, (TMember)member!)!;
}
