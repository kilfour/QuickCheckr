using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.D_ShallowObjectCopy;

[DocFile]
public class ShallowObjectCopy : CheckrTest<ShallowObjectCopy>
{
    protected override bool Asserts => false;
    protected override bool Report => false;


    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Box))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document(a => a.Run(458149048));

    [CodeExample]
    public sealed class Box { public int Value { get; set; } }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from box in Checkr.Input("box",
            from v in Fuzzr.Int(-10, 10)
            select new Box { Value = v }, Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from expect in Checkr.Expect("Value must be >= 0", () => box.Value >= 0)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Value must be >= 0", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("box", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Value: -9 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ Value: -1 }", article.Execution(1).Input(1).Read().Redux.Value);

    }
}