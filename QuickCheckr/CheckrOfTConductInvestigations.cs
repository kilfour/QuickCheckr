using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood;
using QuickCheckr.EvidenceGathering;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr;

public static class CheckrOfTConductInvestigations
{
    public static void Conduct(
        this CheckrOf<Case> checkr,
        InvestigationCount investigationCount,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
        => GatherEvidence(checkr, investigationCount, runs, executionsPerRun, configure, false);

    public static void GatherEvidence(
        this CheckrOf<Case> checkr,
        InvestigationCount investigationCount,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure,
        bool ordered)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var vault = Vault.From(config);
        var vaultCaseFiles = vault.Check(checkr, config, false);
        var caseFiles = new List<CaseFile>();
        for (int i = 0; i < investigationCount.NumberOfInvestigations; i++)
        {
            for (int j = 0; j < runs.NumberOfRuns; j++)
            {
                var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions);
                var caseFile = runner.Run();
                if (caseFile.HasEvidence)
                {
                    caseFiles.Add(caseFile);
                }
            }
        }
        var caseFilesToReport = new List<CaseFile>();
        foreach (var caseFile in caseFiles)
        {
            if (vaultCaseFiles.Any(a => DeskOfficer.AreCaseFilesSimilar(caseFile, a, ordered)))
                continue;
            if (caseFilesToReport.Any(a => DeskOfficer.AreCaseFilesSimilar(caseFile, a, ordered)))
                continue;
            caseFilesToReport.Add(caseFile);
        }

        var logPath = GetVaultPath(config.FileAs, "evidence-log.ql");
        foreach (var caseFile in caseFilesToReport)
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

    public static InvestigationCount Investigations(this int numberOfInvestigations) => new(numberOfInvestigations);

    public record InvestigationCount(int NumberOfInvestigations);

    // ---------------------------------------------------------------------------------------------
    // BEHOLD THE DUPLICATION ... from Vault.cs
    private static string GetVaultPath(string fileAs, string filename)
        => Path.Combine(Constants.ReportFolder, fileAs, Constants.EvidenceFolder, filename);
    // ---------------------------------------------------------------------------------------------
}