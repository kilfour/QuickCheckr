using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.T_EverythingMeansLessThanNull.Methods;

[DocFile]
public class TrackrToPool
{
    [Fact]
    [DocExample(typeof(TrackrToPool), nameof(GetCheckr))]
    [DocContent("  Throws:")]
    [DocExample(typeof(TrackrToPool), nameof(GetMessage), "text")]
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
            from one in Trackr.ToPool("answer", 42)
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string GetMessage() =>
@"A storage attempt was made for a 'Pool' of 'System.Int32' using key 'answer', but no such pool was found.

Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
";
}