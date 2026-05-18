using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class Style
{
    public static Flow<Flow> Pluralize(int count, string str) =>
        Pulse.Trace(count > 1 ? $"{str}s" : str);
    public static Flow<Flow> Pluralize(this Flow<Flow> other, int count, string str)
        => other.Then(Pluralize(count, str));

    public static Flow<Flow> Space() => Pulse.Trace(" ");
    public static Flow<Flow> Space(this Flow<Flow> other)
        => other.Then(Space());

    public static Flow<Flow> Indent(int level)
        => Pulse.Trace(new string(' ', level));
    public static Flow<Flow> Indent(this Flow<Flow> other, int level)
        => other.Then(Indent(level));

    public static Flow<Flow> Dash()
        => Pulse.Trace("-");
    public static Flow<Flow> Dash(this Flow<Flow> other)
            => other.Then(Dash());

    public static Flow<Flow> NewLine()
        => Pulse.Trace(Environment.NewLine);
    public static Flow<Flow> NewLine(this Flow<Flow> other)
        => other.Then(NewLine());

    public static Flow<Flow> OnNewLine()
        => NewLine().Then(Space());
    public static Flow<Flow> OnNewLine(this Flow<Flow> other)
        => other.Then(OnNewLine());

    private static Flow<Flow> LineOf(int length) => Pulse.Trace(new string('-', length));
    public static Flow<Flow> DrawTopLine() =>
        Space().Then(LineOf(60));
    public static Flow<Flow> DrawLine() =>
        OnNewLine().Then(LineOf(60));
    public static Flow<Flow> DrawLine(this Flow<Flow> other) =>
        other.Then(DrawLine());

    public static Flow<Flow> DoubleLineOf(int length) =>
        OnNewLine().Then(Pulse.Trace(new string('=', length)));
    public static Flow<Flow> DoubleLineOf(this Flow<Flow> other, int length) =>
        other.Then(DoubleLineOf(length));

    public static Flow<Flow> LabeledValue(string label, object value) =>
        Pulse.Trace($"{label} = {value}");
    public static Flow<Flow> LabeledValue(this Flow<Flow> other, string label, object value) =>
        other.Then(LabeledValue(label, value));

    public static Flow<Flow> Caption(string label) =>
        Pulse.Trace($"{label}:");
    public static Flow<Flow> Caption(this Flow<Flow> other, string label) =>
        other.Then(Caption(label));

    public static Flow<Flow> Exclaim(string label) =>
        Pulse.Trace($"!! {label}");
    public static Flow<Flow> Exclaim(this Flow<Flow> other, string label) =>
        other.Then(Exclaim(label));

    private readonly static Flow<Flow> Separator = Pulse.Trace(", ");
    private static Flow<Flow> InterspersedPrimed(string input) =>
        Pulse
            .When<Joiner>(a => a.NeedsSeparator(), Separator)
            .Trace(input);

    public static Flow<Flow> Interspersed(IEnumerable<string> input) =>
        Pulse.Scoped<Joiner>(a => a.Start(),
            Pulse.ToFlow(InterspersedPrimed, input));
    public static Flow<Flow> Interspersed(this Flow<Flow> other, IEnumerable<string> input) =>
        other.Then(Interspersed(input));
}