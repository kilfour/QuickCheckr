using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<string> Text(this SelectrOf<StringSlice> selection) =>
       state =>
       {
           var slice = selection(state).Value;
           if (slice == null) return SelectrResult.Default<string>(state);
           return SelectrResult.Some(state, slice.Text);
       };
}
