using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("OneOfPoolWhen with two pools")]
[DocContent("Throws a `NoMatchingPoolElementException` when no pair from the two pools matches the predicate.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(E_OneOfPoolWhen), nameof(Message))]
public class E_OneOfPoolWhen : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var checkr =
            from number in Trackr.ToPool("store number", () => 1)
            from text in Trackr.ToPool("store text", () => "two")
            from selected in Trackr.OneOfPoolWhen<int, string>(
                "pool pair",
                (candidateNumber, candidateText) => candidateNumber.ToString() == candidateText,
                (_, _) => state => CheckrResult.CaseOnly(state))
            select Case.Closed;

        var ex = Assert.Throws<NoMatchingPoolElementException>(() => checkr.Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Trackr.OneOfPoolWhen could not choose a matching pair of 'Int32' and 'String' from pool 'pool pair'.

OneOfPoolWhen needs at least one matching pair whenever it runs.
Check the predicate, or use Trackr.PoolWhen when a match is optional.";
}
