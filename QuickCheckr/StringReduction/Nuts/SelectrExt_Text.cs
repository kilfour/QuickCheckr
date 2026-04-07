using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction.Nuts;

public static partial class SelectrExt
{
    public static SelectrOf<string> Text(this SelectrOf<ISelection> selection) =>
       state =>
       {
           var slice = selection(state).Value;
           if (slice == null) return SelectrResult.Default<string>(state);
           return SelectrResult.Some(state, slice.GetText(state.Text));
       };
}
