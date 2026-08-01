using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    /// <summary>
    /// Creates a stateful run that produces successive candidates for the supplied text.
    /// Use when executing a Stringr script directly rather than registering it as a reducer.
    /// </summary>
    public static SelectrRun Run(this SelectrOf<Selection> selectr, string text)
        => new(selectr, text);
}
