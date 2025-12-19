using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.X_ItIsWritten.Methods;

[DocFile]
public class TrackrStashed
{
    [Fact]
    [DocExample(typeof(TrackrStashed), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(TrackrStashed), nameof(GetMessage), "text")]
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
@"Key 'System.Int32' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank:

- 'Trackr.Stashed'
";

    [Fact]
    [DocExample(typeof(TrackrStashed), nameof(GetCheckrKeyed))]
    [DocContent("  Throws:")]
    [DocExample(typeof(TrackrStashed), nameof(GetMessageKeyed), "text")]
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
            from one in Trackr.Stashed("reused", () => 42)
            from two in Trackr.Stashed("reused", () => 666)
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

- 'Trackr.Stashed'
";

}