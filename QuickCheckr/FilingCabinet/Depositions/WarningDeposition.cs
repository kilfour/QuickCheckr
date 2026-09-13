using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.FilingCabinet.Depositions;

public sealed record WarningDeposition(Warning Warning)
{
    public string Value => Warning.Message;
};
