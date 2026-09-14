using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.K_NestedLists;

[DocFile]
[DocContent(
@"
This tests the performance of shrinking a list of lists, testing the false
property that the sum of lengths of the element lists is at most 10.

The reason this is interesting is that it has lots of local minima under
pure deletion based approaches. e.g. `[[0], ..., [0]]` and `[[0, ..., 0]]` are
both minima for this under anything that can only make individual elements
smaller.

Some libraries, e.g. Hypothesis and jqwik, can shrink this reliably to
a single list of 11 elements: `[[0, 0, 0, 0, 0, 0, 0, 0, 0, 0]]`.

")]
public class NestedLists : QuickTestrPropertyTest<NestedLists>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1959968277")]
    protected override void GetTestr(Journalist journalist) =>
        Testr
            .Named("The sum of lengths of the element lists is at most 10.")
            .For(Fuzzr.Int().Many(0, 20).ToList().Many(0, 20).ToList())
            .Deliberate(a => a.Count)
            .Assert(a => a.Sum(a => a.Count) <= 10)
            .StoreCaseFiles(journalist)
            .Run(1959968277);

    protected override void Verify(Article article)
    {
        Assert.Equal("The sum of lengths of the element lists is at most 10.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(31, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ _, _, _, _, _, _, _, _, _, _, _ ] ]", article.Execution(1).Input(1).Read().Value);
    }
}
