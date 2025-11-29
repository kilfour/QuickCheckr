using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.Methods;


[DocFile]
public class H_Pool
{
    [Fact(Skip = "to be discussed")]
    [DocContent("- `Trackr.Pool`:")]
    [DocExample(typeof(H_Pool), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(H_Pool), nameof(GetMessage), "text")]
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
            from one in Trackr.Pool<int>()
            from two in Trackr.Pool<int>()
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'reused' is written more than once in a single 'Trackr.Pool' run.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Trackr.Pool' uses a unique key.
";
}