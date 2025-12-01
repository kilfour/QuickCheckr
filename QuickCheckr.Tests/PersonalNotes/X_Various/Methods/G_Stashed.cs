using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.PersonalNotes.X_Various.Methods;

[DocFile]
public class G_Stashed
{
    [Fact]
    [DocContent("- `Trackr.Stashed`:")]
    [DocExample(typeof(G_Stashed), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(G_Stashed), nameof(GetMessage), "text")]
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
            from one in Trackr.Stashed(() => 42)
            from two in Trackr.Stashed(() => 666)
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'System.Int32' is written more than once in a single 'Trackr.Stashed' run.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Trackr.Stashed' uses a unique key.
";

}