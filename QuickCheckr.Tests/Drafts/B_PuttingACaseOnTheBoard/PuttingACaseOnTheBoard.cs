using QuickCheckr.Tests.Docs.A_Guide;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.B_RunningACheckr;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration;
using QuickCheckr.Tests.Drafts.E_ConductingInvestigations;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard;

[DocFile]
[DocFileHeader("Putting a Case on the Board")]
[DocNavigation(typeof(Guide), null, null)]
[DocLink(typeof(RunningACheckr))]
[DocLink(typeof(Configuration))]
[DocLink(typeof(GatheringEvidence))]
[DocContent(
@"This chapter takes you through the different options on how to run, configure and rerun Checkrs.
")]
[DocHeader("Contents")]
[DocContent(
@"
- [Running a Checkr][RunningACheckr]
- [Configuration][Configuration]

QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit.
There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**,
which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces,
and the reason for failure.  

Because the failure is surfaced as a standard exception,
any test framework can treat it as a normal assertion failure and you get a rich,
human-readable report for free.
")]
public class PuttingACaseOnTheBoard;