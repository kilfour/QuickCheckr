using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.P_WhenThingsGoWrong.X_ItIsWritten.Methods;


[DocFile]
public class TrackrPoolFor
{
    [Fact]
    [DocExample(typeof(TrackrPoolFor), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(TrackrPoolFor), nameof(GetMessage), "text")]
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
@"Key 'System.Int32' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank:

- 'Trackr.PoolFor'
";
}