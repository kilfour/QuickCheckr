using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.StringReduction;
using QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers;

[DocFile]
[DocContent(
@"Another non-stateful testing example. And one where a Reducer is absolutely necessary.

I used this one, during a course I was teaching to verify student code,
because I did not feel like combing through all the code looking for bugs manually. 

")]
public class ParsingNumbers : CheckrTest<ParsingNumbers>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

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
    public override void Example() =>
        Run(() => TheCheckr().Configure(a => Configure(a)).Run(640099888));

    [CodeExample]
    private CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            Deliberation = new Deliberation(cf => cf.InputsNamed<string>("Expression", a => a.Length), 4),
            ReportMode = config.ReportMode & ~ReportMode.Labels
        };

    private readonly char[] operators = ['+', '-', '*', '/', '^'];

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Expression",
            ParseFuzzr.Expression(),
            Shrink.OnType<string>(s => s.Simplify(
                // remove whitespace
                Stringr.AtleastOnce(char.IsWhiteSpace).Remove().Defined(),
                // remove delimiters
                Stringr.Balanced('(', ')').Replace(t => t[1..^1]).Defined(),
                // remove lhs and op
                from lhs in Stringr.AtleastOnce(char.IsDigit).Remove()
                from op in Stringr.Once(operators).Remove()
                select Selection.Defined,
                // remove op and rhs
                from op in Stringr.Once(operators).Remove()
                from rhs in Stringr.AtleastOnce(char.IsDigit).Remove()
                select Selection.Defined,
                // clean up unary minus noise
                Stringr.AtleastOnce('-').Replace(t => "-").Defined())))
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
