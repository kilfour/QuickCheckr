

using System.Diagnostics;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Authoring.ThePress;

public static class TheJournalist
{
    [StackTraceHidden]
    public static Article Unearths(CaseFile caseFile) => new(caseFile!);

    [StackTraceHidden]
    public static Article Exposes(Action run)
    {
        try
        {
            run();
        }
        catch (FalsifiableException ex)
        {
            return Unearths(ex.TheCaseFile);
        }
        throw new InvalidOperationException("Expected FalsifiableException, but none was thrown.");
    }

    [StackTraceHidden]
    public static Article Investigates(Func<CaseFile> run)
    {
        try
        {
            return Unearths(run());
        }
        catch (FalsifiableException ex)
        {
            return Unearths(ex.TheCaseFile);
        }
    }

    public static void Transcribes(Article article) =>
        Signal.From<Article>(TheEditor.ProofReads)
            .SetArtery(FileLog.Write())
            .Pulse(article);
}
