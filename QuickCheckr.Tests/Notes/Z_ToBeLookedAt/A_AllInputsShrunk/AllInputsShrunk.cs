using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.A_AllInputsShrunk;

[DocFile]
public class AllInputsShrunk : CheckrTest<AllInputsShrunk>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    [DocContent("")]
    [DocContent("")]
    public override void Example() =>
        Document(a => a.Run(1098916919));


    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("An Input", Fuzzr.Int())
        from expect in Checkr.Expect("Always fails", () => false)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Always fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(1).Read().Value);
    }
}
