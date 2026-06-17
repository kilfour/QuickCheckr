using QuickCheckr.Diagnostics.Bolts;
using QuickCheckr.UnderTheHood;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Diagnostics;

public class AutopsyProbe
{
    private readonly Signal<IReadOnlyList<string>>? signal;
    public bool IsEmpty => signal is null;
    private AutopsyProbe() { }
    private AutopsyProbe(IArtery artery, Func<IReadOnlyList<string>, bool>? filter)
    {
        static Flow<Flow> TraceFlow(IReadOnlyList<string> a) => Pulse.Trace(Prepare(a));
        signal = filter is null
            ? Signal.From<IReadOnlyList<string>>(TraceFlow)
            : Signal.From<IReadOnlyList<string>>(a => Pulse.ToFlowIf(filter(a), TraceFlow, () => a));
        signal.SetArtery(artery);
    }
    private static FileLogArtery DefaultArtery() => FileLog.Write("autopsy-report.log");
    public static AutopsyProbe Empty => new();
    public static AutopsyProbe Default => new(DefaultArtery(), null);
    public static AutopsyProbe From(IArtery artery, Func<IReadOnlyList<string>, bool>? filter = null)
        => new(artery, filter);
    public static AutopsyProbe StartsWith(params AutopsyPathBuilder[] builders)
        => new(DefaultArtery(), a => builders.Any(b => Prepare(a).StartsWith(Prepare(b.Parts))));
    public static AutopsyProbe Contains(string filter)
        => new(DefaultArtery(), a => Prepare(a).Contains(filter));
    public static AutopsyProbe ExecutionShrinking
        => new(DefaultArtery(), a => Prepare(a).StartsWith(AutopsyPath.ExecutionShrinking));
    public static AutopsyProbe ActionShrinking
        => new(DefaultArtery(), a => Prepare(a).StartsWith(AutopsyPath.ActionShrinking));

    private static string Prepare(IReadOnlyList<string> strings)
        => string.Join('.', strings);

    public void Observe(CheckrState state, IReadOnlyList<string> paths)
    {
        signal?.Pulse(paths);
        if (focuses.Count > 0)
        {
            var path = Prepare(paths);
            var currentFocuses = focuses.ToList();
            foreach (var focus in currentFocuses)
            {
                if (path.StartsWith(focus))
                {
                    focuses.Remove(focus);
                    action(state);
                }
            }
        }
    }

    private record Focus(string Path, bool NeedsBreak);

    private Action<CheckrState> action = a => { };
    private List<string> focuses = [];

    public AutopsyProbe On(Action<CheckrState> action, params AutopsyPathBuilder[] builders)
    {
        this.action = action;
        focuses = [.. builders.Select(a => Prepare(a.Parts))];
        return this;
    }

    public AutopsyProbe On(Action action, params AutopsyPathBuilder[] builders)
    {
        return On(_ => action(), builders);
    }
}



