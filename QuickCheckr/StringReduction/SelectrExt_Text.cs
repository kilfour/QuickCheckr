using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class SelectrExt
{
    public static SelectrOf<string> Text(this SelectrOf<SelectrSlice> selection) =>
       state =>
       {
           var slice = selection(state).Value;
           if (slice == null) return SelectrResult.Default<string>(state);
           return SelectrResult.Some(state, slice.GetText(state.Text));
       };
}
