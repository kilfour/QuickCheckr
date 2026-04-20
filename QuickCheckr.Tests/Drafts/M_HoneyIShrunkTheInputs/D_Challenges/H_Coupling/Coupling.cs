using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.D_Challenges.H_Coupling;

[DocFile]
[DocContent(
@"
In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
")]
public class Coupling : CheckrTest<Coupling>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(124252565);

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with
        {
            DeliberationPolicy = caseFile =>
                (caseFile.InputCount == 0 ? 100 : 0) +
                caseFile.InputsNamed<List<int>>("List", a => a.Count),
            DeliberationTarget = 2,
            ReportMode = config.ReportMode & ~ReportMode.StackTrace
        };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from length in Checkr.Input("Length", Fuzzr.Int(1, 100))
        from list in Checkr.Input("List", () =>
            from enumerable in Fuzzr.Int(0, length - 1).Many(length)
            select enumerable.ToList())
        from trace in Checkr.Trace("Original", () => list)
        from expect in Checkr.ExpectWhen("No two different indexes point to each other.",
            () => list.All(element => element < list.Count),
            () => list.All(element =>
            {
                int index = list[element];
                if (index != element && list[index] == element)
                    return false;
                return true;
            }))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("No two different indexes point to each other.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 1, 0 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 2, 1, 0, 1 ]", article.Execution(1).Trace(1).Read().Value);
    }
}
