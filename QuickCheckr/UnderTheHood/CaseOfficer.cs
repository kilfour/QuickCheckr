using QuickCheckr.Diagnostics;
using QuickCheckr.FilingCabinet;

namespace QuickCheckr.UnderTheHood;

public class CaseOfficer
{
    public static Findings Investigate(
        CheckrOf<Case> checkr,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CheckrRunnerConfig config,
        AutopsyProbe microtome)
    {
        var findings = new Findings();
        var deliberator = new Deliberator(config.Deliberation);
        for (int i = 0; i < runs.NumberOfRuns; i++)
        {
            var runner = new CheckrRunner(checkr, config, executionsPerRun.NumberOfExecutions, microtome);
            var caseFile = runner.Run();
            findings.Add(caseFile);
            if (caseFile.HasEvidence)
            {
                if (config.Deliberation != null)
                {
                    deliberator.Check(caseFile);
                    if (deliberator.TargetReached())
                        return findings.AddFalsifyingCaseFile(caseFile);
                }
                else
                    return findings.AddFalsifyingCaseFile(caseFile);
            }
        }
        if (config.Deliberation != null && deliberator.HasCaseFile)
            return findings.AddFalsifyingCaseFile(deliberator.GetCaseFile());
        return findings;
    }
}
