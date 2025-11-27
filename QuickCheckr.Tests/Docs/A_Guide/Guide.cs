using QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOut;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide;

[DocFile]
[DocLink(typeof(CheckringThingsOut), "CheckringThingsOut")]
[DocLink(typeof(BoltingHorses), "BoltingHorses")]
[DocHeader("Contents")]
[DocContent(
@"
- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.
")]
public class Guide { }