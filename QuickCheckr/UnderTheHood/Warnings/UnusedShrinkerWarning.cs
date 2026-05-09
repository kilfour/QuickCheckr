using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public record UnusedShrinkerWarning(List<Type> Types, string Key) : Warning(WarningLevel.Debug)
{
    public override string Message => $"Unused {Noun()} found for '{TypesString()}' at key '{Key}'.";
    private string Noun() => Types.Count == 1 ? "shrinker" : "shrinkers";
    private string TypesString() => string.Join(",", Types.Select(a => a.Name));
}
