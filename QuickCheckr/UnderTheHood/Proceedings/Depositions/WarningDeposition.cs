using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Proceedings.Depositions;

public record WarningDeposition(Warning Warning)
{
    public string Value => Warning.Message;
};
