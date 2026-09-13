namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;

/// <summary>
/// Describes a member that QuickCheckr can traverse and reconstruct while shrinking an object.
/// </summary>
public interface ITraversalOption
{
    Type TargetType { get; }
    string MemberName { get; }
    Type MemberType { get; }
    object? Get(object target);
    object Replace(object target, object? member);
}

/// <summary>
/// Describes a member traversal for objects of type <typeparamref name="T"/>.
/// </summary>
public abstract record TraversalOption<T> : ITraversalOption
{
    public Type TargetType => typeof(T);
    public abstract string MemberName { get; }
    public abstract Type MemberType { get; }
    public abstract object? Get(object target);
    public abstract object Replace(object target, object? member);
}

/// <summary>
/// Traverses a selected member and reconstructs its containing object after the member is reduced.
/// </summary>
public sealed record MemberTraversalOption<T, TMember>(
    string Name,
    Func<T, TMember> Getter,
    Func<T, TMember, T> Replacer)
    : TraversalOption<T>
{
    public override string MemberName => Name;
    public override Type MemberType => typeof(TMember);
    public override object? Get(object target) => Getter((T)target);
    public override object Replace(object target, object? member) =>
        Replacer((T)target, (TMember)member!)!;
}
