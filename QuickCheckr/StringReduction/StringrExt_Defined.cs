namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<Selection> Defined<T>(this SelectrOf<T> selection) =>
        from _ in selection select Selection.Defined;
}
