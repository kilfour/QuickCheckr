using QuickCheckr.Tests.Drafts.X_ItIsWritten.Methods;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.T_EverythingMeansLessThanNull.Methods;


[DocFile]
public class TrackrGetPool
{
    [Fact]
    [DocExample(typeof(TrackrGetPool), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(TrackrGetPool), nameof(GetMessage), "text")]
    public void RunTest()
    {
        var ex = Assert.Throws<PoolDoesNotExistException>(() => GetCheckr().Run());
        Assert.Equal(GetMessage(), ex.Message);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from one in Trackr.GetPool<int>()
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"A retrieval attempt was made for a 'Pool' of 'System.Int32', but none was found.

Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
";
}