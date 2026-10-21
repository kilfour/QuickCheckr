using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.F_TheDeepEnd;


public class PoolrAddMultipleLabels : CheckrTest<PoolrAddMultipleLabels>
{
    protected override bool Report => false;
    protected override bool Asserts => false;

    [Fact]
    public override void Example() =>
        Document(a => a.Run());

    protected override CheckrOf<Case> TheCheckr() =>
        from pool1 in Poolr.Add("one", Checkr.Input("input one", Fuzzr.Constant(1)))
        from pool2 in Poolr.Add("two", Checkr.Input("input two", Fuzzr.Constant(2)))
        from all in Poolr.All<int>()
        from _ in Checkr.Expect("Count", a => all.Count == 2 ? a.Pass() : a.Fail(a.Show(all)))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Count", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}
