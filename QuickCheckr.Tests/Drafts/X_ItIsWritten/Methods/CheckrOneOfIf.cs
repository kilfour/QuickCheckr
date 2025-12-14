using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.X_ItIsWritten.Methods;

[DocFile]
public class CheckrOneOfIf
{
    [Fact]
    [DocContent("- `Checkr.OneOfIf`:")]
    [DocExample(typeof(CheckrOneOfIf), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(CheckrOneOfIf), nameof(GetMessage), "text")]
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
            from one in Checkr.OneOfIf((() => true, Checkr.Act("One", () => { })))
            from two in Checkr.OneOfIf((() => true, Checkr.Act("Two", () => { })))
            select Case.Closed;
    }


    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank using the same key:

- 'Checkr.OneOf'
- 'Checkr.OneOfIf'
";
}