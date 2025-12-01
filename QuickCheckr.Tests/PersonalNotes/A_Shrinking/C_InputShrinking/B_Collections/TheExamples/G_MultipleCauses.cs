using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class G_MultipleCauses : BaseTest
{
    [Fact]
    [DocExample(typeof(G_MultipleCauses), nameof(GetCheckr))]
    [DocCodeFile("G_MultipleCauses.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1238961515 }));
        WriteAsserts(article);
        WriteReport<G_MultipleCauses>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("input.Count() > 2, () => !input.Contains(1)", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, _, 1 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 1, 2, 1 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from substite in Trackr.StandIn(666)
            from input in Checkr.Input("Input", Fuzzr.Constant<IEnumerable<int>>([1, 2, 1]))
            from trace in Checkr.Trace("List", () => input)
            from spec in Checkr.SpecIf("input.Count() > 2, () => !input.Contains(1)",
                () => input.Count() > 2, () => !input.Contains(1))
            select Case.Closed;
    }
}