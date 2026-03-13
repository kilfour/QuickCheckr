using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.K_NestedLists;

[DocFile]
public class NestedLists : CheckrTest<NestedLists>
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
    public override void Example() =>
        Run(1716272349);//Run(10.Runs());

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with
        {
            DeliberationPolicy = caseFile =>
                caseFile.InputsNamed<List<List<int>>>("Nested List", a => a.Count),
        };


    [CodeSnippet]
    private static readonly FuzzrOf<List<int>> InnerFuzzr =
         from list in Fuzzr.Int().Many(0, 20)
         select list.ToList();

    private static readonly FuzzrOf<List<List<int>>> TheFuzzr =
         from list in InnerFuzzr.Many(0, 20)
         select list.ToList();

    //from trace in Checkr.Trace("Original", () => input)
    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Nested List", TheFuzzr)
        from expect in Checkr.Expect(
            "The sum of lengths of the element lists is at most 10",
            () => input.Sum(a => a.Count) <= 10)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("The sum of lengths of the element lists is at most 10", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(30, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Nested List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ _, _, _, _, _, _, _, _, _, _, _ ] ]", article.Execution(1).Input(1).Read().Value);
    }
}