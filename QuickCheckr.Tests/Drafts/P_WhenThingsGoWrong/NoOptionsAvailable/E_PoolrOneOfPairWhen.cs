using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("Poolr.OneOfWhen with two pools")]
[DocContent("Throws a `NoMatchingPoolElementException` when no pair from the two pools matches the predicate.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(E_PoolrOneOfPairWhen), nameof(Message))]
public class E_PoolrOneOfPairWhen : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var checkr =
            from number in Poolr.Add("store number", Checkr.Capture(() => 1))
            from text in Poolr.Add("store text", Checkr.Capture(() => "two"))
            from selected in Poolr.OneOfWhen<int, string>(
                "pool pair",
                (candidateNumber, candidateText) => candidateNumber.ToString() == candidateText)
            select Case.Closed;

        var ex = Assert.Throws<NoMatchingPoolElementException>(() => checkr.Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Poolr.OneOfWhen could not choose a matching pair of 'Int32' and 'String' from pool 'pool pair'.

OneOfWhen needs at least one matching pair whenever it runs.
Check the predicate, or use Poolr.When when a match is optional.";
}
