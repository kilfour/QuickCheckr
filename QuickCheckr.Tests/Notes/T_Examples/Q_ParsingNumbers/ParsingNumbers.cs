using QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers;

[DocFile]
public class ParsingNumbers : BaseTest
{
    [Fact] // (Skip = "decimal separator breaks build")
    [DocExample(typeof(ParsingNumbers), nameof(GetCheckr))]
    [DocCodeFile("ParsingNumbers.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(406406480));
        WriteAsserts(article);
        WriteReport<ParsingNumbers>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Match", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Golden Result", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("SUT Result", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("Expression", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("\"2^-2^2\"", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("(2 * 1 - 3 * 2 * 1 * 3 + 1 / 2 * 1) ^ - 2 ^ 2 / 3 ^ 2 / 3 - - 3 / 2", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Compare", article.Execution(1).Trace(2).Read().Label);
        //Assert.Equal("Golden: 1,5000006416666136 /= SUT: 2139,2800925925926", article.Execution(1).Trace(2).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Expression", ParseFuzzr.Expression(), a => a with { Reducer = ParseReducer.Reducer })
            from trace in Checkr.Trace("Original", () => input)
            from goldenResult in Checkr.Act("Golden Result", () => LostIn.Translation(input).Eval())
            from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
            from traceResult in Checkr.Trace("Compare", () => $"Golden: {goldenResult} /= SUT: {result}")
            from match in Checkr.ExpectWhen("Match",
                () => !double.IsNaN(goldenResult),
                () => goldenResult == result)
            select Case.Closed;
    }
}

