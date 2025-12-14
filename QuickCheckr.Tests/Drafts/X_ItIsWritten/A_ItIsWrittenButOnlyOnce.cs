using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.X_ItIsWritten;

[DocFile]
[DocFileHeader("It Is Written, ... But Only Once")]
[DocContent(
@"In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
")]
public class A_ItIsWrittenButOnlyOnce;