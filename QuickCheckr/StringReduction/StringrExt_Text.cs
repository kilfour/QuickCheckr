using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    /// <summary>
    /// Projects a concrete string slice to its selected text.
    /// Use when only the textual value of a low-level slice selector is needed.
    /// </summary>
    public static SelectrOf<string> Text(this SelectrOf<StringSlice> selection) =>
       state =>
       {
           var result = selection(state);
           if (!result.Succeeded)
               return SelectrResult.Default<string>(state);

           return SelectrResult.Some(state, result.Value.Text);
       };
}
