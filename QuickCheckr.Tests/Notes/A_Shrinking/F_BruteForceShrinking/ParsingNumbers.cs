using QuickCheckr.Protocol;
using QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers;
using QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.F_BruteForceShrinking;

[DocFile]
public class ParsingNumbers : BaseTest
{
    [Fact]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(ParsingNumbers), nameof(GetCheckr))]
    [DocHeader("The Configuration:")]
    [DocExample(typeof(ParsingNumbers), nameof(Configure))]
    [DocHeader("The Report:")]
    [DocCodeFile("ParsingNumbers.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(200.Runs(), Configure));
        WriteAsserts(article, false);
        WriteReport<ParsingNumbers>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Match", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(6, ((string)article.Execution(1).Input(1).Read().Value).Length);
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

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            DeliberationPolicy = (a, b) =>
                GetExpressionInputLength(a) < GetExpressionInputLength(b) ? a : b
        };

    private static int GetExpressionInputLength(CaseFile caseFile)
        => ((string)caseFile.Verdict!.ExecutionDepositions[0].InputDepositions[0].Value).Length;
}

