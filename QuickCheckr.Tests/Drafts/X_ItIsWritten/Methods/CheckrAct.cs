using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Templates;


[DocFile]
public class CheckrAct
{
    [Fact]
    [DocContent("- `Checkr.Act`:")]
    [DocExample(typeof(CheckrAct), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(CheckrAct), nameof(GetMessage), "text")]
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
            from one in Checkr.Act("reused", () => { })
            from two in Checkr.Act("reused", () => { })
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