using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Protocol;

public record Investigation(Func<IArtery> Officer, Flow<IFiling> Procedure);
