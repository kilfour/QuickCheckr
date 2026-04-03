using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class TheClerk
{
    public static string Transcribes(CaseFile caseFile, Func<CaseFile, Flow<Flow>> styleGuide)
    {
        return
            Signal.From(styleGuide)
                .SetArtery(Text.Capture())
                .Pulse(caseFile)
                .GetArtery<StringSink>()
                .Content();
    }
}