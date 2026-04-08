using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class StringrExtensions
{
    public static SelectrOf<ITextSelection> Or(this SelectrOf<ITextSelection> left, SelectrOf<ITextSelection> right) =>
        state =>
        {
            if (state.OutOfBounds()) return SelectrResult.Default<ITextSelection>(state);
            state.SnapShot();
            var leftResult = left(state);
            if (leftResult.Value != null && !state.RunStopped) return leftResult;
            state.RestoreSnapShot();
            state.RunStopped = false;
            var rightResult = right(state);
            if (rightResult.Value != null && !state.RunStopped) return rightResult;
            state.RunStopped = true;
            return SelectrResult.Default<ITextSelection>(state);
        };
}
