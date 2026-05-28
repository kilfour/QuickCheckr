using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTConductInvestigations
{
    public static IRecord Conduct(
        this CheckrOf<Case> checkr,
        InvestigationCount maxInvestigations,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
        => checkr.Conduct(maxInvestigations, runs, executionsPerRun, configure, Directive.Default);

    public static IRecord Conduct(
        this CheckrOf<Case> checkr,
        InvestigationCount maxInvestigations,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure,
        Directive directive)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var vault = Vault.From(config);
        var caseFiles = vault.Check(checkr, config, false).ToList();
        var inquiry = new Inquiry().Add([.. caseFiles]);
        var stop = false;
        if (directive.MaxCaseFiles.HasValue && caseFiles.Count >= directive.MaxCaseFiles.Value)
            stop = inquiry.Stop();
        for (int i = 0; i < maxInvestigations.NumberOfInvestigations && !stop; i++)
        {
            for (int j = 0; j < runs.NumberOfRuns && !stop; j++)
            {
                var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions);
                var caseFile = runner.Run();
                inquiry.Add(caseFile);
                if (caseFile.HasEvidence)
                {
                    if (directive.Reject != null && directive.Reject(caseFile))
                        continue;
                    if (!caseFiles.Any(a => Equals(directive.ClassifyBy(a), directive.ClassifyBy(caseFile))))
                    {
                        caseFiles.Add(caseFile);
                    }
                    if (directive.MaxCaseFiles.HasValue && caseFiles.Count >= directive.MaxCaseFiles.Value)
                        stop = inquiry.Stop();
                }
            }
        }

        foreach (var caseFile in caseFiles)
        {
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                config.Custodian.FileRecord(EvidenceVault.GetVaultPath(config.FileAs, $"report_{caseFile.Seed}.qr"), caseFile, config.Clerk.File);
            }
            vault.AddSeed(caseFile.Seed, caseFile.OriginalRunExecutionCount);
        }
        var fileName = Path.Combine(Constants.ReportFolder, config.FileAs, $"report.qr");
        inquiry.AddDistinctFailure([.. caseFiles]);
        config.Custodian.FileRecord(fileName, inquiry, config.Clerk.File);
        return inquiry;
    }

    public static InvestigationCount Investigations(this int numberOfInvestigations) => new(numberOfInvestigations);

    public record InvestigationCount(int NumberOfInvestigations);
}