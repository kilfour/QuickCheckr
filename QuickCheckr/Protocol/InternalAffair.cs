using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Protocol;

public record InternalAffair(Func<IArtery> Officer, Flow<IFiling> Procedure);
