using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTRunColdCases
{
    [StackTraceHidden]
    public static void ReviewColdCases(this CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure)
        => Check(checkr, configure, false);

    [StackTraceHidden]
    public static void CloseResolvedColdCases(this CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure)
        => Check(checkr, configure, true);

    [StackTraceHidden]
    private static void Check(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> configure, bool closeResolved)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var vault = Vault.From(config);
        var caseFiles = vault.Check(checkr, config, closeResolved);
        var review = new ColdCasesReview().Add([.. caseFiles]);
        config.Custodian.FileRecord(config.FileAs, review, config.Clerk.File);
        if (caseFiles.Any(a => a.HasEvidence))
            throw new FalsifiableException(review, config.Clerk.File);
    }
}
