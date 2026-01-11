namespace QuickCheckr.UnderTheHood.Warnings;

public abstract record Warning
{
    public abstract string Message { get; }
}
