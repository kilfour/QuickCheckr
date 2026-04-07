namespace QuickCheckr.StringReduction;

public static partial class SelectrExt
{
    public static SelectrOf<Selection> Defined<T>(this SelectrOf<T> selection) =>
        from _ in selection select Selection.Defined;
}
