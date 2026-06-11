using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Bolts;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Y_Async.K_AsyncActWhen;

[DocFile]
public class AsyncActWhen : CheckrTest<AsyncActWhen>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Flip))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run());

    [CodeSnippet]
    [CodeRemove("return")]
    protected override CheckrOf<Case> TheCheckr() =>
        from flag in Trackr.Stashed(() => new Cell<bool>(false))
        from act in Checkr.ActWhen("Add", () => false, () => Flip.ThisFlag(flag))
        from expect in Checkr.Expect("Fails when flag == False", () => flag.Value)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Fails when flag == False", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [CodeExample]
    public class Flip
    {
        public static Task<bool> ThisFlag(Cell<bool> flag)
        {
            return Task.Run(() =>
            {
                flag.Value = !flag.Value;
                return flag.Value;
            });
        }
    }
}
