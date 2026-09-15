using QuickPulse.Explains;
using QuickPulse.Explains.Abstractions;

namespace QuickCheckr.Tests.Tools;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class DocNavigationAttribute(Type root, Type? previous, Type? next) : Attribute, IDocAttribute
{
    public IEnumerable<DocFragmentAttribute> Expand()
    {
        yield return new DocLinkAttribute(root);
        if (previous is not null)
            yield return new DocLinkAttribute(previous);
        if (next is not null)
            yield return new DocLinkAttribute(next);
        yield return new DocContentAttribute(
$"""
|{GetLink("◀", previous)}|[△][{root.Name}]|{GetLink("▶", next)}|
|---|---|---|

""");
    }

    private static string GetLink(string label, Type? link)
    {
        if (link is null)
            return label;
        return $"[{label}][{link.Name}]";
    }
}