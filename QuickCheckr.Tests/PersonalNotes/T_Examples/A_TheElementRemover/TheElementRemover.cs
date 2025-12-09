using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.PersonalNotes.T_Examples.A_TheElementRemover;

[DocFile]
[DocContent("This test focuses on a simple faulty implementation:")]
[DocExample(typeof(ElementRemover))]
[DocContent("`ElementRemover` only removes the *first* occurrence of a value in a list.")]
[DocContent(
@"Using one input, one action, and one specification,
the test captures the minimal failing case where the list contains multiple copies of the target element.")]
[DocExample(typeof(TheElementRemover), nameof(GetCheckr))]
[DocContent(
@"QuickCheckr reduces the failing run to a list like `[ 5, 5 ]` with `5` as the element to remove,
exposing the bug clearly: the output still contains `5`.  
The generated report shows the input, the attempted deletion, a trace of the original list before shrinking and a trace of the result from the action.  
")]
[DocCodeFile("TheElementRemover.txt", "text")]
public class TheElementRemover : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(438724296));
        WriteAsserts(article);
        WriteReport<TheElementRemover>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("List Does Not Contain Element", article.FailedSpec());

        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());

        Assert.Equal(1, article.Execution(1).Read().ExecutionId);

        Assert.Equal("Delete from List", article.Execution(1).Action(1).Read().Label);

        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 5, 5 ]", article.Execution(1).Input(1).Read().Value);

        Assert.Equal("Item to Remove", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("5", article.Execution(1).Input(2).Read().Value);

        Assert.Equal("List Before Shrinking", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 2, 8, 0, 5, 5, 1, 8, 2, 9 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return listFuzzr;")]
    private static FuzzrOf<List<int>> GetFuzzr()
    {
        var listFuzzr =
            from listLength in Fuzzr.Int(0, 10)
            from enumerable in Fuzzr.Int(0, 10).Many(listLength)
            select enumerable.ToList();
        return listFuzzr;
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from list in Checkr.Input("List", GetFuzzr())
            from element in Checkr.Input("Item to Remove", Fuzzr.Int(0, 10))
            from output in Checkr.Act("Delete from List",
                () => ElementRemover.Remove([.. list], element))
            from listDoesNotContainElement in Checkr.Spec(
                "List Does Not Contain Element",
                () => !output.Contains(element))
            from traceList in Checkr.Trace("List Before Shrinking", () => list)
            select Case.Closed;
    }

    [CodeExample]
    public static class ElementRemover
    {
        public static List<int> Remove(List<int> list, int item)
        {
            list.Remove(item);
            return list;
        }
    }
}