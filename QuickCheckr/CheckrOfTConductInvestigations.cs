using QuickCheckr.EvidenceGathering;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTConductInvestigations
{
    public static void Conduct(
        this CheckrOf<Case> checkr,
        InvestigationCount investigationCount,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
        => Conduct(checkr, investigationCount, runs, executionsPerRun, configure, false);

    public static void Conduct(
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

        foreach (var caseFile in caseFilesToReport)
        {
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                config.Custodian.FileRecord(EvidenceVault.GetVaultPath(config.FileAs, $"report_{caseFile.Seed}.qr"), caseFile, config.Clerk.File);
            }
            vault.AddSeed(caseFile.Seed, caseFile.OriginalRunExecutionCount);
        }
    }

    public static InvestigationCount Investigations(this int numberOfInvestigations) => new(numberOfInvestigations);

    public record InvestigationCount(int NumberOfInvestigations);
}