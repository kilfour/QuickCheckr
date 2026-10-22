using System.Diagnostics;
using QuickCheckr.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;

namespace QuickCheckr;

public static class CheckrOfTConductInvestigations
{
    [StackTraceHidden]
    public static ConfiguredCheckr Conduct(
        this ConfiguredCheckr configuredCheckr,
        InvestigationCount maxInvestigations,
        RunCount runs,
        ExecutionCount executionsPerRun)
        => configuredCheckr.Conduct(maxInvestigations, runs, executionsPerRun, Directive.Default);

    [StackTraceHidden]
    public static ConfiguredCheckr Conduct(
        this ConfiguredCheckr configuredCheckr,
        InvestigationCount maxInvestigations,
        RunCount runs,
        ExecutionCount executionsPerRun,
        Directive directive)
        => configuredCheckr.ConductAsync(maxInvestigations, runs, executionsPerRun, directive).GetAwaiter().GetResult();

    /// <summary>Conducts investigations, awaiting saved-case replay, execution, and shrinking.</summary>
    [StackTraceHidden]
    public static Task<ConfiguredCheckr> ConductAsync(
        this ConfiguredCheckr configuredCheckr,
        InvestigationCount maxInvestigations,
        RunCount runs,
        ExecutionCount executionsPerRun,
        CancellationToken cancellationToken = default)
        => configuredCheckr.ConductAsync(maxInvestigations, runs, executionsPerRun, Directive.Default, cancellationToken);

    /// <summary>Conducts investigations using the supplied directive and cooperative cancellation.</summary>
    [StackTraceHidden]
    public static async Task<ConfiguredCheckr> ConductAsync(
        this ConfiguredCheckr configuredCheckr,
        InvestigationCount maxInvestigations,
        RunCount runs,
        ExecutionCount executionsPerRun,
        Directive directive,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var config = configuredCheckr.Config with { CancellationToken = cancellationToken };
        var vault = Vault.From(config);
        var checkedCaseFiles = (await vault.CheckAsync(configuredCheckr.Checkr, config, false).ConfigureAwait(false)).ToList();
        var caseFiles = checkedCaseFiles
            .Where(caseFile => caseFile.HasEvidence && !caseFile.Rejected)
            .ToList();
        var inquiry = new Inquiry().Add([.. checkedCaseFiles]);
        var stop = NeedsToStop(directive, caseFiles, inquiry);
        for (int i = 0; i < maxInvestigations.NumberOfInvestigations && !stop; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var findings = await CaseOfficer.InvestigateAsync(configuredCheckr.Checkr, runs, executionsPerRun, config, AutopsyProbe.Empty).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            inquiry.Add([.. findings.CaseFiles]);
            foreach (var caseFile in Candidates(findings))
            {
                if (directive.Reject != null && directive.Reject(caseFile))
                    continue;

                var currentIndex = caseFiles.FindIndex(
                    current => Equals(directive.Distinguish(current), directive.Distinguish(caseFile)));
                if (currentIndex == -1)
                    caseFiles.Add(caseFile);
                else if (IsPreferred(caseFile, caseFiles[currentIndex]))
                    caseFiles[currentIndex] = caseFile;

                stop = NeedsToStop(directive, caseFiles, inquiry);
                if (stop)
                    break;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        foreach (var caseFile in caseFiles)
        {
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                config.Custodian.FileEvidence(config.FileAs, caseFile.Seed, caseFile, config.Clerk.File);
            }
            vault.AddSeed(caseFile.Seed, caseFile.OriginalRunExecutionCount);
        }
        inquiry.AddDistinctFailure([.. caseFiles]);
        config.Custodian.FileRecord(config.FileAs, inquiry, config.Clerk.File);
        if (inquiry.DistinctFailureCount > 0)
            throw new FalsifiableException(inquiry, config.Clerk.File);
        return configuredCheckr;
    }

    private static bool NeedsToStop(Directive directive, List<CaseFile> caseFiles, Inquiry inquiry)
    {
        return directive.MaxCaseFiles is { } max && caseFiles.Count >= max && inquiry.Stop();
    }

    private static IEnumerable<CaseFile> Candidates(Findings findings)
    {
        var failingCaseFiles = findings.CaseFiles
            .Where(caseFile => caseFile.HasEvidence && !caseFile.Rejected)
            .ToList();
        return failingCaseFiles.Any(caseFile => caseFile.HasDistinction)
            ? failingCaseFiles
            : findings.FalsifyingCaseFile is { } caseFile
                ? [caseFile]
                : [];
    }

    private static bool IsPreferred(
        CaseFile candidate,
        CaseFile current)
    {
        if (candidate.DeliberationScore is not { } candidateScore ||
            current.DeliberationScore is not { } inlineCurrentScore)
            return false;

        return !TargetReached(inlineCurrentScore, current.DeliberationTarget) &&
               candidateScore < inlineCurrentScore;
    }

    private static bool TargetReached(int score, int? target)
        => target.HasValue && score <= target.Value;

    public static InvestigationCount Investigations(this int numberOfInvestigations) => new(numberOfInvestigations);

    public record InvestigationCount(int NumberOfInvestigations);
}
