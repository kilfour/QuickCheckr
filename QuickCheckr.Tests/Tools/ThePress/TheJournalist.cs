

using System.Diagnostics;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Tests.Tools.ThePress;

public static class TheJournalist
{
    [StackTraceHidden]
    public static Article Unearths(CaseFile caseFile) => new(caseFile!);

    [StackTraceHidden]
    public static Article Exposes(Action run)
    {
        var ex = Assert.Throws<FalsifiableException>(run);
        return Unearths(ex.TheCaseFile);
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
        Signal.From(TheEditor.ProofReads)
            .SetArtery(FileLog.Write())
            .Pulse(article);
}
