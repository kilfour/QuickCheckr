using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.Methods;


[DocFile]
public class H_Pool
{
    [Fact]
    [DocContent("- `Trackr.PoolFor`:")]
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
            from one in Trackr.PoolFor<int>()
            from two in Trackr.PoolFor<int>()
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'System.Int32' is written more than once in a single 'Trackr.PoolFor' run.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Trackr.PoolFor' uses a unique key.
";
}