using System.Diagnostics;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood;

public class Microtome
{
    private readonly Signal<IReadOnlyList<string>>? signal;
    public bool IsEmpty => signal is null;
    private Microtome() { }
    private Microtome(IArtery artery, Func<IReadOnlyList<string>, bool>? filter)
    {
        static Flow<Flow> TraceFlow(IReadOnlyList<string> a) => Pulse.Trace(Prepare(a));
        signal = filter is null
            ? Signal.From<IReadOnlyList<string>>(TraceFlow)
            : Signal.From<IReadOnlyList<string>>(a => Pulse.ToFlowIf(filter(a), TraceFlow, () => a));
        signal.SetArtery(artery);
    }
    private static IArtery DefaultArtery() => FileLog.Write("autopsy-report.log");
    public static Microtome Empty => new();
    public static Microtome Default => new(DefaultArtery(), null);
    public static Microtome From(IArtery artery, Func<IReadOnlyList<string>, bool>? filter = null)
        => new(artery, filter);
    public static Microtome StartsWith(string filter)
        => new(DefaultArtery(), a => Prepare(a).StartsWith(filter));
    public static Microtome Contains(string filter)
        => new(DefaultArtery(), a => Prepare(a).Contains(filter));

    public static string Prepare(IReadOnlyList<string> strings)
        => string.Join('.', strings);

    public void Observe(IReadOnlyList<string> paths)
    {
        signal?.Pulse(paths);
        foreach (var focus in focuses)
        {
            if (focus(paths))
                Debugger.Break();
        }
    }

    private Func<IReadOnlyList<string>, bool>[] focuses = [];

    public Microtome BreakOn(params Func<IReadOnlyList<string>, bool>[] focuses)
    {
        this.focuses = focuses;
        return this;
    }
}



