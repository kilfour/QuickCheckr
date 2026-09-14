using QuickCheckr;
using QuickCheckr.UnderTheHood;

namespace QuickTestr;


public abstract record StringCase(string Label)
{
    public static StringCaseValue Null => new("Null", null!);
    public static StringCaseValue Empty => new("Empty", "");
    public static StringCaseValue Whitespace => new("Whitespace", " ");
    public static StringCaseMaxLength LongerThan(int maxLength)
        => new("Too Long", maxLength);

    public abstract CheckrOf<Case> GetCheckr<TResult>(Func<string, TResult> factory);
    public abstract CheckrOf<DelayedResult<TResult>> GetActCheckr<TResult>(Func<string, TResult> factory);

    public static List<string> GetMessage<T>(string input, DelayedResult<T> delayedResult)
    {
        if (delayedResult.Threw)
            return [$"{input}: {delayedResult.Exception!.GetType().Name}", $"{delayedResult.Exception.Message}"];
        return [$"{input}: Succes"];
    }
}

public record StringCaseValue(string Label, string Value) : StringCase(Label)
{
    public override CheckrOf<Case> GetCheckr<TResult>(Func<string, TResult> factory) =>
        from collector in Trackr.GetStashed<List<string>>()
        from result in Checkr.ActCarefully(Label, () => factory(Value))
        from storeMessage in Checkr.Perform(() => collector.AddRange(GetMessage(Label, result)))
        select Case.Closed;

    public override CheckrOf<DelayedResult<TResult>> GetActCheckr<TResult>(Func<string, TResult> factory) =>
        Checkr.ActCarefully(Label, () => factory(Value));
}

public record StringCaseMaxLength(string Label, int Length) : StringCaseValue(Label, new string('a', Length + 1));

