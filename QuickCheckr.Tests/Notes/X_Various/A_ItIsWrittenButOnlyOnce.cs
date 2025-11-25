using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;


namespace QuickCheckr.Tests.Notes.X_Various
{
    [DocFile]
    [DocFileHeader("It Is Written, ... But Only Once")]
    [DocContent(
    @"In QuickCheckr, the strings you pass to `Checkr.Input`, `Checkr.Act`, `Checkr.Spec`, etcetera, are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
")]
    public class A_ItIsWrittenButOnlyOnce : BaseTest
    {
        [Fact]
        [DocContent("- `Checkr.Input`:")]
        [DocExample(typeof(A_ItIsWrittenButOnlyOnce), nameof(CheckrInput_GetCheckr))]
        [DocContent("  Throws:")]
        [DocExample(typeof(A_ItIsWrittenButOnlyOnce), nameof(CheckrInput_Message), "text")]
        public void CheckrInput()
        {
            var ex = Assert.Throws<DuplicateKeyWriteException>(() => CheckrInput_GetCheckr().Run());
            Assert.Equal(CheckrInput_Message(), ex.Message.PulseToLog());
        }

        [CodeSnippet]
        [CodeRemove("return")]
        private static CheckrOf<Case> CheckrInput_GetCheckr()
        {
            return
                from one in Checkr.Input("reused", Fuzzr.Int())
                from two in Checkr.Input("reused", Fuzzr.Int())
                from spec in Checkr.Spec("Always passes", () => true)
                select Case.Closed;
        }

        [CodeSnippet]
        [CodeRemove("@\"")]
        [CodeRemove("\";")]
        private static string CheckrInput_Message() =>
@"Key 'reused' is written more than once in a single 'Checkr.Input' execution.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Checkr.Input' uses a unique key.
";
    }
}