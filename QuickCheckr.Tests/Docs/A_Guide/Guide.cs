using QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOut;
using QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions;
using QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;
using QuickCheckr.Tests.Drafts.L_TheLoadedDie;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide;

[DocFile]
[DocLink(typeof(CheckringThingsOut), "CheckringThingsOut")]
[DocLink(typeof(TheStashRunsAndExecutions), "TheStashRunsAndExecutions")]
[DocLink(typeof(ChoosingTheNextMove), "ChoosingTheNextMove")]
[DocLink(typeof(BoltingHorses), "BoltingHorses")]
[DocLink(typeof(TheLoadedDie), "TheLoadedDie")]
[DocContent(
@"
Welcome to the QuickCheckr Guide.  
This is a living document, we start small, with a short introduction and a real-world example.")]
[DocHeader("Contents")]
[DocContent(
@"
- [Checkr-ing Things Out (a quick tour)][CheckringThingsOut]
  -  A minimal Checkr and how to run it.
  -  How to add inputs, actions, and traces.
- [The Stash, Runs and executions][TheStashRunsAndExecutions]
  -  An example and an explanation of how QuickCheckr handles stateful testing.
- [ChoosingTheNextMove]
  -  A brief introduction to behavioural branching.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.
- [TheLoadedDie]
  -  Using QuickCheckr to verify properties that should hold for the entire run.")]
public class Guide { }