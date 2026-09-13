using QuickCheckr.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood;

public class CaseOfficer
{
    public static Findings Investigate(
        CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CheckrRunnerConfig config,
        AutopsyProbe autopsyProbe)
    {
        var findings = new Findings();
        Deliberator? deliberator = config.Deliberation is null
            ? null
            : new Deliberator(config.Deliberation);
        var inlineDeliberation = false;
        int? inlineTarget = null;
        CaseFile? firstDistinguishedCaseFile = null;
        for (int i = 0; i < runs.NumberOfRuns; i++)
        {
            var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions, autopsyProbe);
            var caseFile = runner.Run();
            findings.Add(caseFile);
            if (caseFile.HasEvidence)
            {
                if (deliberator is null && caseFile.DeliberationScore.HasValue)
                {
                    inlineDeliberation = true;
                    inlineTarget = caseFile.DeliberationTarget;
                    deliberator = new Deliberator(
                        new Deliberation(
                            candidate => candidate.DeliberationScore ?? int.MaxValue,
                            caseFile.DeliberationTarget));
                }
                else if (inlineDeliberation &&
                         caseFile.DeliberationTarget != inlineTarget)
                {
                    throw new InvalidOperationException(
                        "Checkr.DeliberateUntil must use the same target in every run.");
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
