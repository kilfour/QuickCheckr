using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickPulse;


namespace QuickCheckr.UnderTheHood;

public class FalsifiableException : Exception
{
    public CaseFile TheCaseFile { get; private set; }

    public FalsifiableException(CaseFile caseFile, Func<CaseFile, Flow<Flow>> styleGuide)
        : base(Environment.NewLine + TheClerk.Transcribes(AddTestMethodInfo(caseFile), styleGuide))
    {
        TheCaseFile = caseFile;
    }

    public FalsifiableException(CaseFile caseFile, Func<CaseFile, Flow<Flow>> styleGuide, Exception? exception)
        : base(Environment.NewLine + TheClerk.Transcribes(AddTestMethodInfo(caseFile), styleGuide), exception)
    {
        TheCaseFile = caseFile;
    }

    private static CaseFile AddTestMethodInfo(CaseFile caseFile)
    {
        var assembly = typeof(Case).Assembly;
        var trace = new StackTrace(fNeedFileInfo: true);
        foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
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