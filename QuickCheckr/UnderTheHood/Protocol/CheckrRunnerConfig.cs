using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Protocol;

[Flags]
public enum ShrinkMode
{
    None = 0,
    Executions = 1,
    Actions = 2,
    Inputs = 4,
    Reduction = 8,
    All = Executions | Actions | Inputs | Reduction,
    Default = Executions | Inputs | Reduction
}

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        Seed = config.Seed;
        Signals = [.. config.Investigations.Select(a => Signal.From(a.Procedure).SetArtery(a.Officer))];
        ShrinkMode = config.ShrinkMode;
        FileAs = config.FileAs;
    }

    public int? Seed { get; init; }
    public Signal<IFiling>[] Signals { get; init; }
    public ShrinkMode ShrinkMode { get; init; }
    public string FileAs { get; init; }
}
