using QuickFuzzr;

namespace QuickTestr.Tests.Notes.T_Exploration;

public static partial class Evilize
{
    public static FuzzrOf<Intent> Strings() =>
        Configr.Primitive(Evilr.String()!);

    public static FuzzrOf<Intent> StringsExcept(
        params Evilr.StringOption[] excluded) =>
        Configr.Primitive(Evilr.StringExcept(excluded)!);

    public static FuzzrOf<T> Then<T>(this FuzzrOf<Intent> source, FuzzrOf<T> other) =>
        from _ in source
        from val in other
        select val;

}

public static partial class Evilr
{
    public static FuzzrOf<string?> String() =>
        StringExcept();

    public static FuzzrOf<string?> StringExcept(
        params StringOption[] excluded)
    {
        var selected = StringFuzzrs
            .Where(entry => !excluded.Contains(entry.Option))
            .Select(entry => entry.Fuzzr)
            .ToArray();

        if (selected.Length == 0)
            throw new ArgumentException(
                "At least one evil string category must remain.",
                nameof(excluded));

        return Fuzzr.OneOf(selected);
    }

    public enum StringOption
    {
        Default,
        Null,
        Empty,
        Whitespace
    }

    private static readonly (StringOption Option, FuzzrOf<string?> Fuzzr)[]
        StringFuzzrs =
        [
            (StringOption.Default, Fuzzr.String()),
            (StringOption.Null, NullString()),
            (StringOption.Empty, EmptyString()),
            (StringOption.Whitespace, WhitespaceString())
        ];

    public static FuzzrOf<string?> NullString() =>
        Fuzzr.Constant<string?>(null);

    public static FuzzrOf<string?> EmptyString() =>
        Fuzzr.Constant<string?>("");

    public static FuzzrOf<string?> WhitespaceString() =>
        Fuzzr.OneOf<string?>(
            " ",
            "\t",
            "\r",
            "\n",
            "\r\n",
            "\u00A0");
}



