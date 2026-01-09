using QuickCheckr.Protocol;
using QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers;
using QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers;

[DocFile]
public class ParsingNumbers : CheckrTest<ParsingNumbers>
{
    protected override bool Asserts => true;
    protected override bool Report => true;

    [Fact]
    [DocModelHeader]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    [DocContent("")]
    [DocContent("")]
    public override void Example() =>
        Run(1259510317);

    [Fact]
    public void Review()
        => TheCheckr().ReviewColdCases(Configure);

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            DeliberationPolicy = caseFile => caseFile.InputsNamed("Expression", a => a.Length),
            DeliberationTarget = 6,
            FileAs = "ParsingNumbers",
            Custodian = Custodian.Local(),
            ReportMode = config.ReportMode & ~ReportMode.Labels,
        };

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
