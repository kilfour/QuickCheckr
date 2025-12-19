using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.X_ItIsWritten.Methods;

[DocFile]
public class CheckrInput
{
    [Fact]
    [DocExample(typeof(CheckrInput), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(CheckrInput), nameof(GetMessage), "text")]
    public void RunTest()
    {
        var ex = Assert.Throws<DuplicateKeyWriteException>(() => GetCheckr().Run());
        Assert.Equal(GetMessage(), ex.Message);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from one in Checkr.Input("reused", Fuzzr.Int())
            from two in Checkr.Input("reused", Fuzzr.Int())
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'reused' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank:

- 'Checkr.Input'
";
}