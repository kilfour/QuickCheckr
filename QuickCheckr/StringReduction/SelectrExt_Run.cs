using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class SelectrExt
{
    public static SelectrRun Run(this SelectrOf<Selection> selectr, string text)
        => new(selectr, text);
}
