using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr;

public static class CheckrOfTGatherEvidence
{
    public static void GatherEvidence(
        this CheckrOf<Case> checkr,
        CheckrOfTRun.RunCount runs,
        CheckrOfTRun.ExecutionCount executionsPerRun,
        Func<CheckrConfig, CheckrConfig> configure)
    {
        var config = new CheckrRunnerConfig(configure(CheckrConfig.Default()));
        var entries = GetVaultEntries(config);
        var caseFiles = new List<CaseFile>();

        foreach (var entry in entries)
        {
            var vaultConfig = config with { Seed = entry.Seed };
            var caseFile = new CheckrRunner(checkr, config, entry.ExecutionsPerRun).Run();
            if (caseFile.HasVerdict())
            {
                caseFiles.Add(caseFile);
            }
        }

        for (int i = 0; i < runs.NumberOfRuns; i++)
        {
            var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions);
            var caseFile = runner.Run();
            if (caseFile.HasVerdict())
            {
                caseFiles.Add(caseFile);
            }
        }
        var caseFilesToReport = new List<CaseFile>();
        foreach (var caseFile in caseFiles)
        {
            if (caseFilesToReport.Any(a => a.IsSimilar(caseFile)))
                continue;
            caseFilesToReport.Add(caseFile);
        }
        var vault = Vault.From(config);
        var logPath = GetVaultPath(config.FileAs, "evidence-log.ql");
        var logRecorder = config.Custodian.Recorder(logPath);
        foreach (var caseFile in caseFilesToReport)
        {
            logRecorder.Append($"Run failed for Seed: {caseFile.Verdict!.Seed}.");
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                config.Custodian.Recorder(GetVaultPath(config.FileAs, $"report_{caseFile.Verdict!.Seed}.qr"))
                    .Append(TheClerk.Transcribes(caseFile));
            }
            vault.AddSeed(caseFile.Verdict!.Seed, caseFile.Verdict!.OriginalRunExecutionCount, caseFile.Verdict!.GetFailure());
        }



        // if (finalCaseFile.HasVerdict())
        //     ReportCaseFile(executionsPerRun, config, finalCaseFile);
        // else
        //     ReportCaseFileSummary(config, caseFiles);
        // return CaseSummary.FromCaseFiles(caseFiles);
    }

    // ---------------------------------------------------------------------------------------------
    // BEHOLD THE DUPLICATION ... from Vault.cs
    private static HashSet<VaultEntry> GetVaultEntries(CheckrRunnerConfig config)
    {
        return config.Custodian.Retriever(GetVaultPath(config.FileAs, "evidence.qv"))
            .Select(ParseVaultEntry)
            .Where(entry => entry != null)
            .ToHashSet()!;
    }

    private static string GetVaultPath(string fileAs, string filename)
        => Path.Combine(Constants.ReportFolder, fileAs, Constants.EvidenceFolder, filename);

    private static VaultEntry ParseVaultEntry(string line)
    {
        var parts = line.Split(',');
        return parts.Length == 3 &&
            int.TryParse(parts[0], out var seed) &&
            int.TryParse(parts[1], out var executions)
            ? new VaultEntry(seed, executions, parts[2])
            : null!;
    }
    // ---------------------------------------------------------------------------------------------
}