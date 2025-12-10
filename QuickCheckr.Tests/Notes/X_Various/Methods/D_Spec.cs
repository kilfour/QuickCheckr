using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.Methods;


[DocFile]
public class D_Spec
{
    [Fact]
    [DocContent("- `Checkr.Spec`:")]
    [DocExample(typeof(D_Spec), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(D_Spec), nameof(GetMessage), "text")]
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
            from one in Checkr.Spec("reused", () => true)
            from two in Checkr.Spec("reused", () => true)
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'reused' is written more than once in a single 'Checkr.Spec' execution.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Checkr.Spec' uses a unique key.
";
}