using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.ShrinkCount.Executions;

[DocFile]
public class OneExecutionShrunk : CheckrTest<OneExecutionShrunk>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run(1459888731, 2.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Counter", Fuzzr.Counter("counter"))
        from expect in Checkr.Expect("Fails on 2", () => input != 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(1, article.ShrinkCount);
    }
}