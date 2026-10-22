using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Shrinking;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.CustomShrinkersIncludes.ConditionalShrinkers;

[DocFile]
public class OnSpecificValue : CheckrTest<OnSpecificValue>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(1.Runs()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 5)),
            Shrinkr.For<int>()
                .When(s => s.Value(a => a == 5))
                .Reduce(Reduce.Towards(0)))
        from trace in Checkr.Trace("Original", a => a.Show(inter))
        from expect in Checkr.Expect("All ints smaller than 1.", () => inter.A < 1 && inter.B < 1)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("All ints smaller than 1.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Inter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ B: 5 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ B: 1 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(["{ A: 42, B: 5 }"], article.Execution(1).Trace(1).Read().Value);
    }

    public record Inter(int A, int B);
}