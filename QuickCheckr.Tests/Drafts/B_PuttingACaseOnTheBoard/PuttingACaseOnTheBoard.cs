using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.B_RunningACheckr;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_GatheringEvidence;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.F_TestFrameworkIntegration;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard;

[DocFile]
[DocFileHeader("Putting a Case on the Board")]
[DocLink(typeof(RunningACheckr))]
[DocLink(typeof(Configuration))]
[DocLink(typeof(GatheringEvidence))]
[DocContent(
@"This chapter takes you through the different options on how to run, configure and rerun Checkrs.
It also explains the *Evidence Vault* system and how to gather evidence.
")]
[DocInclude(typeof(TestFrameworkIntegration))]
[DocHeader("Contents")]
[DocContent(
@"
- [Running a Checkr][RunningACheckr]
- [Configuration][Configuration]
- [The Evidence Vault][TheEvidenceVault]
- [Gathering Evidence][GatheringEvidence]
")]
public class PuttingACaseOnTheBoard;