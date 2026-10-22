using QuickCheckr.Protocol;
using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr.UnderTheHood;

internal static class SpokesPerson
{
    [StackTraceHidden]
    public static void Announce(CheckrConfig config, Findings findings, ExecutionCount executionsPerRun)
    {
        if (findings.FalsifyingCaseFile is { } caseFile)
            AnnounceCaseFile(executionsPerRun, config, caseFile);
        else
            AnnounceFindings(config, findings);
    }

    [StackTraceHidden]
    private static void AnnounceCaseFile(ExecutionCount executionsPerRun, CheckrConfig config, CaseFile caseFile)
    {
        if (!string.IsNullOrWhiteSpace(config.FileAs))
        {
            config.Custodian.FileRecord(config.FileAs, caseFile, config.Clerk.File);
            Vault.From(config).AddSeed(caseFile.Seed, executionsPerRun.NumberOfExecutions);
        }
        throw new FalsifiableException(caseFile, config.Clerk.File);
    }

    private static void AnnounceFindings(CheckrConfig config, Findings findings)
    {
        if (!string.IsNullOrWhiteSpace(config.FileAs))
        {
            config.Custodian.FileRecord(config.FileAs, findings, config.Clerk.File);
        }
    }
}
