using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.E_CoupledPredicate;

[DocFile]
public class A_CoupledPredicate : CheckrTest<A_CoupledPredicate>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Pair))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(2017105380);

    [CodeExample]
    record Pair(int A, int B);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(9, 9)))
        from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("A + B <= 10", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        // Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("pair", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 9, B: 9 }", article.Execution(1).Input(1).Read().Value);
    }
}
