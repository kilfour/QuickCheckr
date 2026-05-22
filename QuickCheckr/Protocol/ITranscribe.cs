using QuickCheckr.FilingCabinet;
using QuickPulse;

namespace QuickCheckr.Protocol;

public interface ITranscribe
{
    Flow<Flow> File(IRecord record);
}



