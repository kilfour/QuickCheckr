using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public abstract record Warning(WarningLevel WarningLevel = WarningLevel.Important)
{
    public abstract string Message { get; }
}
