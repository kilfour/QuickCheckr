using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickPulse;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public class CourtClerk : ITranscribe
{
    public static CourtClerk Default() => new();
    private Decorum decorum = new();

    public CourtClerk WithLabels()
    {
        decorum = decorum with { Labels = true };
        return this;
    }

    public CourtClerk WithStackTrace()
    {
        decorum = decorum with { StackTrace = true };
        return this;
    }

    public Flow<Flow> File(IRecord record)
        => TheCourtStyleGuide.Record(record, decorum);
}
