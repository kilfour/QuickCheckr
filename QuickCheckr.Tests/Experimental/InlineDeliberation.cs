using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental;

public class InlineDeliberation : CheckrTest<InlineDeliberation>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example()
        => Document(a => a.Run(764624976));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from list in Checkr.Input("The List",
            from length in Fuzzr.Int()
            from enumerable in Fuzzr.Int(0, length - 1).Many(length)
            select enumerable.ToList())
        from deliberate in Checkr.Deliberate(() => list.Count)
        from until in Checkr.DeliberateUntil(2)
        from expect in Checkr.Expect(
            "No two different indexes point to each other.",
            () => list.All(
                element =>
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
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 1, 0 ]", article.Execution(1).Input(1).Read().Value);
    }
}
