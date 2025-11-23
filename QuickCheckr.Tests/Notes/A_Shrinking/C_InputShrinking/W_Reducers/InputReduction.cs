using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.W_Reducers;

[DocFile]
[DocExample(typeof(InputReduction), nameof(GetCheckr))]
[DocExample(typeof(InputReduction), nameof(TowardsZero))]
[DocCodeFile("InputReduction.txt", "text")]
public class InputReduction : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1681332589 }));
        WriteAsserts(article);
        WriteReport<InputReduction>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Int should be smaller than 42", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Int", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Int", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("79", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from i in Checkr.Input("Int", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
            from spec in Checkr.Spec("Int should be smaller than 42", () => i < 42)
            from trace in Checkr.Trace("Int", i)
            select Case.Closed;
    }

    [CodeExample]
    private static IEnumerable<int> TowardsZero(int input)
    {
        var local = input;
        while (local != 0)
        {
            if (local > 0)
                yield return --local;
            else
                yield return ++local;
        }
    }
}