using QuickCheckr.Protocol;
using QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers;

[DocFile]
public class ParsingNumbers : CheckrTest<ParsingNumbers>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    [DocBoldHeader("The Fuzzr")]
    [DocExample(typeof(ParseFuzzr))]
    [DocBoldHeader("The Reducer")]
    [DocExample(typeof(ParseReducer))]
    public override void Example() =>
        Run(200.Runs());

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            DeliberationPolicy = caseFile => caseFile.InputsNamed("Expression", a => a.Length),
            DeliberationTarget = 6,
            ReportMode = config.ReportMode & ~ReportMode.Labels
        };

    // FileAs = "ParsingNumbers",
    // Custodian = Custodian.Local(),

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Expression",
                ParseFuzzr.Expression(),
                Reduce.Function<string>(ParseReducer.Reducer, true))
        from goldenResult in Checkr.Act("Golden Result", () => LostIn.Translation(input).Eval())
        from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
        from traceResult in Checkr.Trace("Compare", () => $"Golden: {goldenResult} /= SUT: {result}")
        from match in Checkr.ExpectWhen("Match",
            () => !double.IsNaN(goldenResult),
            () => goldenResult == result)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(6, article.Execution(1).Input(1).Read().Redux.Value!.Length);
    }
}
