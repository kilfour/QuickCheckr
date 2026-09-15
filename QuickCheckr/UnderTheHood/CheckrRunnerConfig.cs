using QuickCheckr.Protocol;
using QuickCheckr.Protocol.Custodians;

namespace QuickCheckr.UnderTheHood;

internal sealed record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        FileAs = config.FileAs;
        Custodian = config.Custodian;
        ShrinkMode = config.ShrinkMode;
        ReportMode = config.ReportMode;
        WarningLevel = config.WarningLevel;
        Clerk = config.Clerk;
        MaxStepsPerReducer = config.MaxStepsPerReducer;
    }

    public int? Seed { get; set; } = null;
    public CancellationToken CancellationToken { get; init; }
    public string FileAs { get; }
    public ICustodian Custodian { get; }
    public ShrinkMode ShrinkMode { get; }
    public ReportMode ReportMode { get; }
    public WarningLevel WarningLevel { get; }
    public ITranscribe Clerk { get; }
    public int MaxStepsPerReducer { get; }
}
