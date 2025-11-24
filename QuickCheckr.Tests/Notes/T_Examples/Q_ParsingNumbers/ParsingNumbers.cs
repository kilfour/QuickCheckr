using QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers;

//[DocFile]
public class ParsingNumbers : BaseTest
{
    //[Fact]
    [DocExample(typeof(ParsingNumbers), nameof(GetCheckr))]
    [DocCodeFile("ParsingNumbers.txt", "text")]
    public void RunTest()
    {
        //ParseFuzzr.Expression(2).Many(5).Generate().PulseToLog();
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), a => a with { Seed = null }));
        //WriteAsserts(article, true);
        WriteReport<ParsingNumbers>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {

    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", ParseFuzzr.Expression(), a => a with { Reducer = a => [a] })
            from result in Checkr.Act("Result", () => LostIn.Translation(input.Text).Eval())
            from spec in Checkr.Spec("Parser evaluates correctly", () => result == input.Value)
            from traceExpression in Checkr.Trace("Expression", input.Text)
            from traceValue in Checkr.Trace("Desired Result", input.Value)
            from traceResult in Checkr.Trace("Result", result)
            select Case.Closed;
    }
}






