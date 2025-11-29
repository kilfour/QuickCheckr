using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class G1_MultipleCausesDefaultInSpec : BaseTest
{
    [Fact]
    [DocExample(typeof(G1_MultipleCausesDefaultInSpec), nameof(GetCheckr))]
    [DocContent(
@"The shrinking in the following report is too greedy, this is because the shrinking strategy for collections replaces
irrelevant values with `default`, this guards against greedy shrinking in most cases, but not if the spec asserts on a default value.")]
    [DocCodeFile("G1_MultipleCausesDefaultInSpec.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1238961515 }));
        WriteAsserts(article);
        WriteReport<G1_MultipleCausesDefaultInSpec>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("input.Count() > 2, () => !input.Contains(0)", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, _, _ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 0, 2, 0 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.Constant<IEnumerable<int>>([0, 2, 0]))
            from trace in Checkr.Trace("List", () => input)
            from spec in Checkr.SpecIf("input.Count() > 2, () => !input.Contains(0)",
                () => input.Count() > 2, () => !input.Contains(0))
            select Case.Closed;
    }
}