using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTConductInvestigations
{
    [StackTraceHidden]
    public static IRecord Conduct(
        this CheckrOf<Case> checkr,
        InvestigationCount maxInvestigations,
        RunCount runs,
        ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
        => checkr.Conduct(maxInvestigations, runs, executionsPerRun, configure, Directive.Default);

    [StackTraceHidden]
    public static IRecord Conduct(
        this CheckrOf<Case> checkr,
        InvestigationCount maxInvestigations,
        RunCount runs,
        ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure,
        Directive directive)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var vault = Vault.From(config);
        var caseFiles = vault.Check(checkr, config, false).ToList();
        var inquiry = new Inquiry().Add([.. caseFiles]);
        var stop = NeedsToStop(directive, caseFiles, inquiry);
        for (int i = 0; i < maxInvestigations.NumberOfInvestigations && !stop; i++)
        {
            var findings = CaseOfficer.Investigate(checkr, runs, executionsPerRun, config, Microtome.Empty);
            inquiry.Add([.. findings.CaseFiles]);
            if (findings.FalsifyingCaseFile is { } caseFile)
            {
                if (directive.Reject != null && directive.Reject(caseFile))
                    continue;
                if (!caseFiles.Any(a => Equals(directive.ClassifyBy(a), directive.ClassifyBy(caseFile))))
                {
                    caseFiles.Add(caseFile);
                }
                stop = NeedsToStop(directive, caseFiles, inquiry);
            }
        }

        foreach (var caseFile in caseFiles)
        {
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                config.Custodian.FileEvidence(config.FileAs, caseFile.Seed, caseFile, config.Clerk.File);
            }
            vault.AddSeed(caseFile.Seed, caseFile.OriginalRunExecutionCount);
        }
        inquiry.AddDistinctFailure([.. caseFiles]);
        config.Custodian.FileRecord(config.FileAs, inquiry, config.Clerk.File);
        if (inquiry.DistinctFailureCount > 0)
            throw new FalsifiableException(inquiry, config.Clerk.File);
        return inquiry;
    }

    private static bool NeedsToStop(Directive directive, List<CaseFile> caseFiles, Inquiry inquiry)
    {
        return directive.MaxCaseFiles is { } max && caseFiles.Count >= max && inquiry.Stop();
    }

    public static InvestigationCount Investigations(this int numberOfInvestigations) => new(numberOfInvestigations);

    public record InvestigationCount(int NumberOfInvestigations);
}