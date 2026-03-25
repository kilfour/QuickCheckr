using QuickCheckr.Protocol;
using QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers;

[DocFile]
[DocContent(
@"Another non-stateful testing example. And one where a Reducer is absolutely necessary.

I used this one, during a course I was teaching to verify student code,
because I did not feel like combing through all the code looking for bugs manually.  
So you could say it's a real world example, and I deliberately did not clean up the code.  

I had a working model, so all that was needed was a Fuzzr, and a way to get to a minimal fail-case.  
The Reducer is far from perfect, but by adding a deliberation policy, this test quickly and reliably came up with said minimal fail-case.  

")]
public class ParsingNumbers : CheckrTest<ParsingNumbers>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent(
@"**Demonstrates:**
- Oracle style testing
- Custom Reducers
- Custom Deliberation Policies
")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    [DocBoldHeader("The Fuzzr")]
    [DocExample(typeof(ParseFuzzr))]
    [DocContent("This Fuzzr generates a valid numeric expression string of variable length.  \n *Some Examples:*")]
    [DocCodeFile("ExpressionExamples.txt", "text")]
    [DocBoldHeader("The Reducer")]
    [DocExample(typeof(ParseReducer))]
    public override void Example() =>
        Run(1166603196);

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            DeliberationPolicy = caseFile => caseFile.InputsNamed<string>("Expression", a => a.Length),
            DeliberationTarget = 4,
            ReportMode = config.ReportMode & ~ReportMode.Labels
        };

    // FileAs = "ParsingNumbers",
    // Custodian = Custodian.Local(),

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Expression",
                ParseFuzzr.Expression(),
                Shrink.OnType<string>(s => s.Simplify(Reduce.Function<string>(ParseReducer.Reducer, true))))
        from oracleResult in Checkr.Act("Oracle Result", () => LostIn.Translation(input).Eval())
        from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
        from traceResult in Checkr.Trace("Compare", () => $"Oracle: {oracleResult} /= SUT: {result}")
        from match in Checkr.ExpectWhen("Match",
            () => !double.IsNaN(oracleResult),
            () => oracleResult == result)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(6, article.Execution(1).Input(1).Read().Redux.Value!.Length);
    }
}
