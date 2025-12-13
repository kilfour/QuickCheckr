using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.Methods;


[DocFile]
public class E_Trace
{
    [Fact]
    [DocContent("- `Checkr.Trace`:")]
    [DocExample(typeof(E_Trace), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(E_Trace), nameof(GetMessage), "text")]
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
            from one in Checkr.Trace("reused", () => "")
            from two in Checkr.Trace("reused", () => "")
            from spec in Checkr.Expect("fails", () => false) // <= this one needs to run
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'reused' is written more than once in a single 'Checkr.Trace' execution.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Checkr.Trace' uses a unique key.
";
}