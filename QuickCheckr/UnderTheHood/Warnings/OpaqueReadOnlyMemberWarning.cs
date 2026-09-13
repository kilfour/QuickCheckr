using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public sealed record OpaqueReadOnlyMemberWarning(Type Type, string Path)
    : Warning(WarningLevel.Verbose)
{
    public override string Message =>
        $"Read-only member of type '{Type.Name}' at '{Path}' has no configured traversal and was retained as an opaque value.";
}
