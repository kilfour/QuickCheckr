using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.Methods;

[DocFile]
public class C_ActCarefully
{
    [Fact]
    [DocContent("- `Checkr.ActCarefully`:")]
    [DocExample(typeof(C_ActCarefully), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(C_ActCarefully), nameof(GetMessage), "text")]
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
@"Key 'reused' is written more than once in a single 'Checkr.Act' execution.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Checkr.Act' uses a unique key.
";
}