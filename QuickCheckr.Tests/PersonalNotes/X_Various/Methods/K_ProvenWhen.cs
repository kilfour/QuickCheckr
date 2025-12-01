using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.PersonalNotes.X_Various.Methods;


[DocFile]
public class K_ProvenWhen
{
    [Fact]
    [DocContent("- `Trackr.ProvenWhen`:")]
    [DocExample(typeof(K_ProvenWhen), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(K_ProvenWhen), nameof(GetMessage), "text")]
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
            from one in Trackr.ProvenWhen("reused", () => false)
            from two in Trackr.ProvenWhen("reused", () => false)
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