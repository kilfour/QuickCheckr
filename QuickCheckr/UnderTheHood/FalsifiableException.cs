using System.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;


namespace QuickCheckr.UnderTheHood;

/// <summary>
/// Represents a falsified Checkr run and exposes the record that explains the failure.
/// </summary>
public sealed class FalsifiableException : Exception
{
    public IRecord TheCaseFile { get; private set; }

    public FalsifiableException(CaseFile caseFile, Func<IRecord, Flow<Flow>> styleGuide)
        : base(Environment.NewLine + TheClerk.Transcribes(AddTestMethodInfo(caseFile), styleGuide))
    {
        TheCaseFile = caseFile;
    }

    public FalsifiableException(CaseFile caseFile, Func<IRecord, Flow<Flow>> styleGuide, Exception? exception)
        : base(Environment.NewLine + TheClerk.Transcribes(AddTestMethodInfo(caseFile), styleGuide), exception)
    {
        TheCaseFile = caseFile;
    }

    public FalsifiableException(ColdCasesReview review, Func<IRecord, Flow<Flow>> styleGuide)
        : base(Environment.NewLine + TheClerk.Transcribes(review, styleGuide))
    {
        TheCaseFile = review;
    }

    public FalsifiableException(Inquiry inquiry, Func<IRecord, Flow<Flow>> styleGuide)
        : base(Environment.NewLine + TheClerk.Transcribes(inquiry, styleGuide))
    {
        TheCaseFile = inquiry;
    }

    private static CaseFile AddTestMethodInfo(CaseFile caseFile)
    {
        var assembly = typeof(Case).Assembly;
        var trace = new StackTrace(fNeedFileInfo: true);
        foreach (var frame in trace.GetFrames() ?? [])
        {
            var file = frame.GetFileName();
            if (!string.IsNullOrEmpty(file))
            {
                var method = frame.GetMethod();
                if (method == null) continue;
                var declaringAssembly = method.DeclaringType?.Assembly;
                if (declaringAssembly == assembly) continue;
                caseFile.AddTestMethodDisposition(
                    new TestMethodInfoDeposition(
                        method?.Name ?? "unknown_test", file, frame.GetFileLineNumber()));
                return caseFile;
            }
        }
        return caseFile;
    }
}
