using QuickCheckr.Diagnostics;
using QuickCheckr.FilingCabinet;

namespace QuickCheckr.UnderTheHood;

internal static class CaseOfficer
{
    public static Findings Investigate(
        CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CheckrRunnerConfig config,
        AutopsyProbe autopsyProbe)
    {
        var findings = new Findings();
        Deliberator? deliberator = null;
        int? inlineTarget = null;
        CaseFile? firstDistinguishedCaseFile = null;
        for (int i = 0; i < runs.NumberOfRuns; i++)
        {
            var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions, autopsyProbe);
            var caseFile = runner.Run();
            findings.Add(caseFile);
            if (caseFile.HasEvidence)
            {
                if (caseFile.Rejected)
                    continue;

                if (deliberator is null && caseFile.DeliberationScore.HasValue)
                {
                    inlineTarget = caseFile.DeliberationTarget;
                    deliberator = new Deliberator(
                        candidate => candidate.DeliberationScore ?? int.MaxValue,
                        caseFile.DeliberationTarget);
                }
                else if (deliberator is not null &&
                         caseFile.DeliberationTarget != inlineTarget)
                {
                    throw new InvalidOperationException(
                        "Trackr.DeliberateUntil must use the same target in every run.");
                }

                if (deliberator is not null)
                {
                    deliberator.Check(caseFile);
                    if (deliberator.TargetReached() && !caseFile.HasDistinction)
                        return findings.AddFalsifyingCaseFile(caseFile);
                }
                else if (caseFile.HasDistinction)
                    firstDistinguishedCaseFile ??= caseFile;
                else
                    return findings.AddFalsifyingCaseFile(caseFile);
            }
        }
        if (deliberator is not null && deliberator.HasCaseFile)
            return findings.AddFalsifyingCaseFile(deliberator.GetCaseFile());
        if (firstDistinguishedCaseFile is not null)
            return findings.AddFalsifyingCaseFile(firstDistinguishedCaseFile);
        return findings;
    }
}
