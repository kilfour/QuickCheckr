using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTGatherEvidence
{
    public static IRecord GatherEvidence(
        this CheckrOf<Case> checkr,
        CheckrOfTConductInvestigations.InvestigationCount maxInvestigations,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure,
        Func<ICaseFileSummary, object> fingerPrint,
        int? MaxCaseFiles = null,
        Func<ICaseFileSummary, bool>? RejectWhen = null)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var vault = Vault.From(config);
        var caseFiles = vault.Check(checkr, config, false).ToList();
        var inquiry = new Inquiry().Add([.. caseFiles]);
        var stop = false;
        for (int i = 0; i < maxInvestigations.NumberOfInvestigations && !stop; i++)
        {
            for (int j = 0; j < runs.NumberOfRuns && !stop; j++)
            {
                var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions);
                var caseFile = runner.Run();
                inquiry.Add(caseFile);
                if (caseFile.HasEvidence)
                {
                    if (RejectWhen != null && RejectWhen(caseFile))
                        continue;
                    if (!caseFiles.Any(a => Equals(fingerPrint(a), fingerPrint(caseFile))))
                    {
                        caseFiles.Add(caseFile);
                    }
                    if (MaxCaseFiles.HasValue && caseFiles.Count > MaxCaseFiles.Value)
                        stop = true;
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
}