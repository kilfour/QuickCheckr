using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.X_ItIsWritten.Methods;


[DocFile]
public class CheckrOneOf
{
    [Fact]
    [DocExample(typeof(CheckrOneOf), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(CheckrOneOf), nameof(GetMessage), "text")]
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
@"Key 'Checkr|OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank:

- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
";

    [Fact]
    [DocExample(typeof(CheckrOneOf), nameof(GetCheckrKeyed))]
    [DocContent("  Throws:")]
    [DocExample(typeof(CheckrOneOf), nameof(GetMessageKeyed), "text")]
    public void RunTestKeyed()
    {
        var ex = Assert.Throws<DuplicateKeyWriteException>(() => GetCheckrKeyed().Run());
        Assert.Equal(GetMessageKeyed(), ex.Message);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrKeyed()
    {
        return
            from one in Checkr.OneOf("reused", Checkr.Act("One", () => { }))
            from two in Checkr.OneOf("reused", Checkr.Act("Two", () => { }))
            select Case.Closed;
    }


    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessageKeyed() =>
@"Key 'reused' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank:

- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
";
}
