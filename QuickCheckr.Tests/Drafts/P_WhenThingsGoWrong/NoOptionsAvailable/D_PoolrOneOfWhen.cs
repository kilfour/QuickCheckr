using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("Poolr.OneOfWhen")]
[DocContent("Throws a `NoMatchingPoolElementException` when no value in the pool matches the predicate.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(D_PoolrOneOfWhen), nameof(Message))]
public class D_PoolrOneOfWhen : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var checkr =
            from stored in Poolr.Add("store", Checkr.Capture(() => 1))
            from selected in Poolr.OneOfWhen<int>(
                "pool",
                value => value > 1)
            select Case.Closed;

        var ex = Assert.Throws<NoMatchingPoolElementException>(() => checkr.Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Poolr.OneOfWhen could not choose a 'Int32' from pool 'pool' because no stored value matched the predicate.

OneOfWhen needs at least one matching stored value whenever it runs.
Check the predicate, or use Poolr.When when a match is optional.";
}
