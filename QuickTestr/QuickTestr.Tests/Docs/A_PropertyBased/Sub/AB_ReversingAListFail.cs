using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Docs.A_PropertyBased.Sub;

[DocFileHeader(" ")]
public class AB_ReversingAListFail : QuickTestrPropertyTest<AB_ReversingAListFail>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocContent("""
That passes, which isn't very interesting.
Let's break it by moving all `42`s to the end.
""")]
    [DocTestr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("174616483")]
    protected override void GetTestr(Journalist journalist)
    {
        Testr.Named("Reverse is its own inverse")
            .For(Fuzzr.Int().Many(1, 10))
            .Assert(s => Reverse(Reverse(s)).SequenceEqual(s))
            .StoreCaseFiles(journalist)
            .Run(174616483);
        // ---------------------------------------------------------------------
        static IEnumerable<int> Reverse(IEnumerable<int> l) => [.. HideTheAnswer(l.Reverse())];
        static IEnumerable<int> HideTheAnswer(IEnumerable<int> l)
            => l.Where(a => a != 42).Concat(l.Where(a => a == 42));
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("Reverse is its own inverse", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(7, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 42, _ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ 86, 33, 42, 21, 7, 62, 44, 10 ]", article.Execution(1).Input(1).Read().Original.Value);
    }
}
