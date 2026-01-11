namespace QuickCheckr.UnderTheHood.Warnings;

public record NullValueWarning(Type Type, string Key) : Warning
{
    public override string Message => $"Null value for type '{Type.Name}' at input '{Key}'.";
}
