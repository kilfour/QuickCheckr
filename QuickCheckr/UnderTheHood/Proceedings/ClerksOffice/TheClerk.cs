using QuickCheckr.FilingCabinet;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class TheClerk
{
    public static string Transcribes(IRecord record, Func<IRecord, Flow<Flow>> styleGuide)
    {
        return
            Signal.From(styleGuide)
                .SetArtery(Text.Capture())
                .Pulse(record)
                .GetArtery<StringSink>()
                .Content();
    }
}