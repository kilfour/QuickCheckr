namespace QuickCheckr.UnderTheHood.Warnings;

public record NoPropertiesWarning(Type Type, string Key) : Warning
{
    public override string Message => $"Object of type '{Type.Name}' has no shrinkable properties at input '{Key}'.";
}
