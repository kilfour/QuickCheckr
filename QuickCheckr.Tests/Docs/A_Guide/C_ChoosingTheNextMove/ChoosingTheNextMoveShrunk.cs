using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;

[DocFileHeader("Shrinking Branching Behaviour")]
public class ChoosingTheNextMoveShrunk : CheckrTest<ChoosingTheNextMoveShrunk>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("And if we let the QuickCheckr shrinker do its work:")]
    [DocReport]
    [DocContent(
@"The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
")]
    public override void Example() => Run(971854269, 10.ExecutionsPerRun());

    protected override CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            ReportMode = config.ReportMode & ~ReportMode.StackTrace
        };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() => ChoosingTheNextMove.GetCheckr();

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: No answers for you.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(5, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
    }
}


