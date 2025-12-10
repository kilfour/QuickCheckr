using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.Methods;


[DocFile]
public class F_OneOf
{
    [Fact]
    [DocContent("- `Checkr.OneOf`:")]
    [DocExample(typeof(F_OneOf), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(F_OneOf), nameof(GetMessage), "text")]
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
            from one in Checkr.OneOf(Checkr.Act("One", () => { }))
            from two in Checkr.OneOf(Checkr.Act("Two", () => { }))
            select Case.Closed;
    }


    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'OneOf|Default|QuickCheckr|Key' is written more than once in a single 'Checkr.OneOf' execution.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each 'Checkr.OneOf' uses a unique key.
";
}