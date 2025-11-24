using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Configuration;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class F_NestedCollections : BaseTest
{
    [Fact]
    [DocExample(typeof(F_NestedCollections), nameof(GetCheckr))]
    [DocCodeFile("F_NestedCollections.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 146712596 }));
        WriteAsserts(article);
        WriteReport<F_NestedCollections>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("List[0][0] is not 42", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ 42 ] ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ [ 42, 1, 15 ], [ 17 ], [ 27, 9 ] ]", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
            from trace in Checkr.Trace("List", input)
            from spec in Checkr.SpecIf("List[0][0] is not 42",
                () => input.Any() && input.First().Any(),
                () => input.First().First() != 42)
            select Case.Closed;
    }
}