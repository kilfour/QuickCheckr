using QuickCheckr.Protocol;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse;

namespace QuickCheckr.UnderTheHood;

public record CheckrRunnerConfig
{
    public CheckrRunnerConfig(CheckrConfig config)
    {
        FileAs = config.FileAs;
        Custodian = config.Custodian;
        ShrinkMode = config.ShrinkMode;
        ReportMode = config.ReportMode;
        DeliberationPolicy = config.DeliberationPolicy;
        DeliberationTarget = config.DeliberationTarget;
        StyleGuide = config.StyleGuide;
        MaxStepsPerReducer = config.MaxStepsPerReducer;
    }

    public int? Seed { get; set; } = null;
    public string FileAs { get; }
    public ICustodian Custodian { get; }
    public ShrinkMode ShrinkMode { get; }
    public ReportMode ReportMode { get; }
    public Func<ICaseFileSummary, int>? DeliberationPolicy { get; }
    public int? DeliberationTarget { get; init; }
    public Func<CaseFile, Flow<Flow>> StyleGuide { get; }
    public int MaxStepsPerReducer { get; }
}
