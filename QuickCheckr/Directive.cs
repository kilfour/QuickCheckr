using QuickCheckr.EvidenceGathering;
using QuickCheckr.FilingCabinet;

namespace QuickCheckr;

public sealed record Directive
{
    public static Directive Default { get; } = new();

    public Func<CaseFile, object> Distinguish { get; init; }
        = a => a.Distinction ?? CaseFileFingerprints.FingerPrint(a);

    public int? MaxCaseFiles { get; init; }

    public Func<CaseFile, bool>? Reject { get; init; }
}
