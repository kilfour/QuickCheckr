using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.EvidenceGathering;

namespace QuickCheckr;

public static class CheckrOfTGatherEvidence
{
    public static void GatherEvidence(
        this CheckrOf<Case> checkr,
        InvestigationCount investigationCount,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
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
                if (caseFile.HasVerdict())
                {
                    caseFiles.Add(caseFile);
                }
            }
        }
        var caseFilesToReport = new List<CaseFile>();
        foreach (var caseFile in caseFiles)
        {
            if (vaultCaseFiles.Any(a => DeskOfficer.AreCaseFilesSimilar(caseFile, a)))
                continue;
            if (caseFilesToReport.Any(a => DeskOfficer.AreCaseFilesSimilar(caseFile, a)))
                continue;
            caseFilesToReport.Add(caseFile);
        }

        var logPath = GetVaultPath(config.FileAs, "evidence-log.ql");
        var logRecorder = config.Custodian.Recorder(logPath);
        foreach (var caseFile in caseFilesToReport)
        {
            // ---------------------------------------------------------------------------------------------
            // BEHOLD THE DUPLICATION ... from Vault.cs
            logRecorder.Append($"Run failed for Seed: {caseFile.Verdict!.Seed}.");
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                config.Custodian.Recorder(GetVaultPath(config.FileAs, $"report_{caseFile.Verdict!.Seed}.qr"))
                    .Append(TheClerk.Transcribes(caseFile));
            }
            vault.AddSeed(caseFile.Verdict!.Seed, caseFile.Verdict!.OriginalRunExecutionCount, caseFile.Verdict!.GetFailure());
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