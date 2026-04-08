namespace QuickCheckr.StringReduction.Nuts;

public static class TextSelectionExtensions
{
    public static StringSlice AsSlice(this ITextSelection selection) =>
        selection is StringSlice slice ? slice : throw new InvalidCastException();
    public static IEditable AsEditable(this ITextSelection selection) =>
        selection is IEditable editable ? editable : throw new InvalidCastException();
}
