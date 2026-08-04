using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.P_WhenThingsGoWrong.X_ItIsWritten.Methods;


[DocFile]
public class TrackrVerify
{
    [Fact]
    [DocExample(typeof(TrackrVerify), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(TrackrVerify), nameof(GetMessage), "text")]
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
            from one in Trackr.Verify("reused", () => true)
            from two in Trackr.Verify("reused", () => true)
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"Key 'reused' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank:

- 'Checkr.Expect'
- 'Checkr.ExpectEquals'
- 'Checkr.ExpectWhen'
- 'Checkr.ExpectThrewExactly'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
";
}
