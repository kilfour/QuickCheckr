using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class SpokesPerson
{
    [StackTraceHidden]
    public static void Announce(CheckrRunnerConfig config, Findings findings, ExecutionCount executionsPerRun)
    {
        if (findings.FalsifyingCaseFile is { } caseFile)
            AnnounceCaseFile(executionsPerRun, config, caseFile);
        else
            AnnounceFindings(config, findings);
    }

    [StackTraceHidden]
    private static void AnnounceCaseFile(ExecutionCount executionsPerRun, CheckrRunnerConfig config, CaseFile caseFile)
    {
        if (!string.IsNullOrWhiteSpace(config.FileAs))
        {
            config.Custodian.FileRecord(ReportPath.Main(config), caseFile, config.Clerk.File);
            Vault.From(config).AddSeed(caseFile.Seed, executionsPerRun.NumberOfExecutions);
        }
        throw new FalsifiableException(caseFile, config.Clerk.File);
    }

    private static void AnnounceFindings(CheckrRunnerConfig config, Findings findings)
    {
        if (!string.IsNullOrWhiteSpace(config.FileAs))
        {
            config.Custodian.FileRecord(ReportPath.Main(config), findings, config.Clerk.File);
        }
    }
}