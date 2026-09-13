using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public sealed record NoPropertiesWarning(Type Type, string Key) : Warning(WarningLevel.Debug)
{
    public override string Message => $"Object of type '{Type.Name}' has no shrinkable properties at input '{Key}'.";
}
