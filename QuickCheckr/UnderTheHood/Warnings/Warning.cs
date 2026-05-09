using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public abstract record Warning(WarningLevel WarningLevel)
{
    public abstract string Message { get; }
}
