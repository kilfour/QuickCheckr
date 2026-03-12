using QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.A_Showr;
using QuickCheckr.Tests.Notes.Z_ToBeLookedAt.K_CheckrsCourt.A_TheCheckrClass;
using QuickCheckr.Tests.Notes.Z_ToBeLookedAt.K_CheckrsCourt.B_TheTrackerClass;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.K_CheckrsCourt;


[DocFile]
[DocLink(typeof(TheCheckrClass))]
[DocLink(typeof(TheTrackerClass))]
[DocLink(typeof(A_TheShowrClass))]
[DocContent(
@"The basic building block of QuickCheckr is a `CheckrOf<T>` (also, informally, called a *Checkr* throughout these docs). 

Multiple Checkrs can be combined into a composite Checkr using `LINQ` chains, as many of the examples show.  

The *atomic* Checkrs are created through a couple of static Factory classes :

- [The Checkr Class][TheCheckrClass]
  - Provides methods to create the *basic* Checkrs.
  - All of the Checkrs created by this class are *execution-scoped*.
- [The Tracker Class][TheTrackerClass]
  - Provides methods to create the Checkrs that deal with state-mutation, and/or Checkrs that are *run-scoped*.  
- [The Tracker Class][TheShowrClass]
  - Allows for customization of the QuickChecker value printer.  
")]
public class CheckrsCourt
{

}