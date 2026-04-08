using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr;

public static class CheckrOfTGatherEvidence
{
    public static void GatherEvidence(
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
        var stop = false;
        for (int i = 0; i < maxInvestigations.NumberOfInvestigations && !stop; i++)
        {
            for (int j = 0; j < runs.NumberOfRuns && !stop; j++)
            {
                var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions);
                var caseFile = runner.Run();
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

        var logPath = GetVaultPath(config.FileAs, "evidence-log.ql");
        foreach (var caseFile in caseFiles)
        {
            // ---------------------------------------------------------------------------------------------
            // BEHOLD THE DUPLICATION ... from Vault.cs
            config.Custodian.LogVaultAction(logPath, $"Run failed for Seed: {caseFile.Seed}.");
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                config.Custodian.StoreCaseFile(GetVaultPath(config.FileAs, $"report_{caseFile.Seed}.qr"), caseFile, config.StyleGuide);
            }
            vault.AddSeed(caseFile.Seed, caseFile.OriginalRunExecutionCount);
            // ---------------------------------------------------------------------------------------------
        }
    }

    // ---------------------------------------------------------------------------------------------
    // BEHOLD THE DUPLICATION ... from Vault.cs
    private static string GetVaultPath(string fileAs, string filename)
        => Path.Combine(Constants.ReportFolder, fileAs, Constants.EvidenceFolder, filename);
    // ---------------------------------------------------------------------------------------------
}