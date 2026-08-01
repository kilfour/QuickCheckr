using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<string> Text(this SelectrOf<StringSlice> selection) =>
       state =>
       {
           var result = selection(state);
           if (!result.Succeeded)
               return SelectrResult.Default<string>(state);

           return SelectrResult.Some(state, result.Value.Text);
       };
}
