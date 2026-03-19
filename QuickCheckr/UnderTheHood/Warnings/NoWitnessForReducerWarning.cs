namespace QuickCheckr.UnderTheHood.Warnings;

public record NoWitnessForReducerWarning(string Key) : Warning
{
    public override string Message => $"No witness for reducer at '{Key}'.";
}
