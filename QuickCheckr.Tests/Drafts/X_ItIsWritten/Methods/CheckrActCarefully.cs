using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Drafts.X_ItIsWritten.Methods;

[DocFile]
public class CheckrActCarefully
{
    [Fact]
    [DocContent("- `Checkr.ActCarefully`:")]
    [DocExample(typeof(CheckrActCarefully), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(CheckrActCarefully), nameof(GetMessage), "text")]
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
            from one in Checkr.ActCarefully("reused", () => { })
            from two in Checkr.ActCarefully("reused", () => { })
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

The following Checkrs write to the this bank using the same key:

- 'Checkr.Act'
- 'Checkr.ActCarefully'
";
}