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
    }

    public int? Seed { get; set; } = null;
    public string FileAs { get; init; }
    public ICustodian Custodian { get; init; }
    public ShrinkMode ShrinkMode { get; init; }
    public ReportMode ReportMode { get; init; }
    public Func<ICaseFileSummary, int>? DeliberationPolicy { get; init; }
    public int? DeliberationTarget { get; init; }
    public Flow<CaseFile> StyleGuide { get; init; }
}
