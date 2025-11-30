using QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOut;
using QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide;

[DocFile]
[DocLink(typeof(CheckringThingsOut), "CheckringThingsOut")]
[DocLink(typeof(TheStashRunsAndExecutions), "TheStashRunsAndExecutions")]
[DocLink(typeof(BoltingHorses), "BoltingHorses")]
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
  -  An example an explanation of how QuickCheckr handles statefull testing.
- [Bolting Horses][BoltingHorses]
  - A real-world example of hunting down a subtle invariant bug.")]
[DocHeader("Coming Soon")]
[DocContent(
@"More chapters will follow as QuickCheckr grows:  
- The Stash, Runs and Executions
- Shrinking 101
- Pools
")]
public class Guide { }