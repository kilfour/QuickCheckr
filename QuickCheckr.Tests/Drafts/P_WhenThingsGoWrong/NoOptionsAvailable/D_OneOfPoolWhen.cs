using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("OneOfPoolWhen")]
[DocContent("Throws a `NoMatchingPoolElementException` when no value in the pool matches the predicate.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(D_OneOfPoolWhen), nameof(Message))]
public class D_OneOfPoolWhen : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var checkr =
            from stored in Poolr.Add("store", Checkr.Capture(() => 1))
            from selected in Trackr.OneOfPoolWhen<int>(
                "pool",
                value => value > 1,
                _ => state => CheckrResult.CaseOnly(state))
            select Case.Closed;

        var ex = Assert.Throws<NoMatchingPoolElementException>(() => checkr.Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Trackr.OneOfPoolWhen could not choose a 'Int32' from pool 'pool' because no stored value matched the predicate.

OneOfPoolWhen needs at least one matching stored value whenever it runs.
Check the predicate, or use Trackr.PoolWhen when a match is optional.";
}
