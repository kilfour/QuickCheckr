using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTRunColdCases
{
    [StackTraceHidden]
    public static ConfiguredCheckr ReviewColdCases(this ConfiguredCheckr configuredCheckr)
        => Check(configuredCheckr, false);

    [StackTraceHidden]
    public static ConfiguredCheckr CloseResolvedColdCases(this ConfiguredCheckr configuredCheckr)
        => Check(configuredCheckr, true);

    [StackTraceHidden]
    private static ConfiguredCheckr Check(ConfiguredCheckr configuredCheckr, bool closeResolved)
    {
        var config = configuredCheckr.Config with { };
        var vault = Vault.From(config);
        var caseFiles = vault.Check(configuredCheckr.Checkr, config, closeResolved);
        var review = new ColdCasesReview().Add([.. caseFiles]);
        config.Custodian.FileRecord(config.FileAs, review, config.Clerk.File);
        if (caseFiles.Any(a => a.HasEvidence))
            throw new FalsifiableException(review, config.Clerk.File);
        return configuredCheckr;
    }
}
